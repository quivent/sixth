use std::path::{Path, PathBuf};
use std::sync::Mutex;

use crate::db::Database;
use crate::models::{
    Gate, Phase, PhaseStatus, TestRun, TestSummary, Word, WordStatus,
};
use crate::parser::{self, WordManifest};
use crate::runner;

/// Central application state, shared across all Tauri commands.
pub struct AppState {
    project_root: PathBuf,
    manifest: Mutex<WordManifest>,
    db: Mutex<Database>,
    /// Cached adversarial test results (test_name -> passed)
    test_cache: Mutex<std::collections::HashMap<String, bool>>,
}

impl AppState {
    pub fn new(project_root: PathBuf, db_path: &Path, manifest_path: &Path) -> Self {
        let db = Database::open(db_path).expect("Failed to open database");
        let manifest = WordManifest::load(manifest_path);

        let state = AppState {
            project_root,
            manifest: Mutex::new(manifest),
            db: Mutex::new(db),
            test_cache: Mutex::new(std::collections::HashMap::new()),
        };

        // Initial scan (includes running tests once)
        state.refresh_tests();
        state.rescan();
        state
    }

    /// Run adversarial tests and cache results. Call sparingly - takes ~1s.
    pub fn refresh_tests(&self) {
        let test_results = runner::run_adversarial_tests(&self.project_root);
        let tests_run = test_results.len();
        let tests_passing = test_results.values().filter(|&&p| p).count();
        eprintln!("[tests] Ran adversarial tests: {}/{} passing", tests_passing, tests_run);

        let mut cache = self.test_cache.lock().unwrap();
        *cache = test_results;
    }

    pub fn project_root(&self) -> &Path {
        &self.project_root
    }

    /// Re-scan ARM64 .fs files and update word statuses.
    /// Uses cached test results - call refresh_tests() to update test pass/fail.
    pub fn rescan(&self) {
        let arm64_dir = self.project_root.join("compiler/shannon/arch/arm64");
        let arm64_words = parser::scan_directory(&arm64_dir);

        // Debug: count ARM64 words
        let total_arm64: usize = arm64_words.values().map(|s| s.len()).sum();
        eprintln!("[rescan] Found {} ARM64 words across {} files", total_arm64, arm64_words.len());

        // Scan adversarial tests for word coverage (fast - just file parsing)
        let adversarial_dir = self.project_root.join("compiler/tests/adversarial");
        let adversarial_tests = parser::scan_adversarial_tests(&adversarial_dir);
        let covered_words = adversarial_tests.len();
        eprintln!("[rescan] {} words with test coverage", covered_words);

        // Use cached test results (don't re-run tests on every file change)
        let test_results = self.test_cache.lock().unwrap().clone();

        let db = self.db.lock().unwrap();
        let overrides = db.get_word_overrides();
        drop(db);

        let mut manifest = self.manifest.lock().unwrap();
        let before_total = manifest.words.len();
        let before_done = manifest.words.iter().filter(|w| w.status == crate::models::WordStatus::Done).count();
        manifest.update_from_scan(&arm64_words, &overrides, &adversarial_tests, &test_results);
        let after_total = manifest.words.len();
        let after_done = manifest.words.iter().filter(|w| w.status == crate::models::WordStatus::Done).count();
        eprintln!("[rescan] Total words: {} (was {}, +{} new from ARM64)",
            after_total, before_total, after_total - before_total);
        eprintln!("[rescan] Done: {}/{} ({:.0}%)",
            after_done, after_total, (after_done as f64 / after_total as f64) * 100.0);
    }

    /// Get current word data (after scan + overrides)
    pub fn get_word_data(&self) -> Vec<Word> {
        let manifest = self.manifest.lock().unwrap();
        manifest.words.clone()
    }

    /// Get adversarial test summary (total, passing) from cache
    pub fn get_adversarial_summary(&self) -> (u32, u32) {
        let cache = self.test_cache.lock().unwrap();
        let total = cache.len() as u32;
        let passing = cache.values().filter(|&&p| p).count() as u32;
        (total, passing)
    }

    /// Get phase data with gate states from DB
    pub fn get_phases(&self) -> serde_json::Value {
        let manifest = self.manifest.lock().unwrap();
        let db = self.db.lock().unwrap();
        let gate_states = db.get_all_gates();

        let mut phases = build_phases();

        // Auto-detect completed phases from ARM64 test results
        let arm64_completed_phases = self.detect_arm64_completed_phases();

        // Update gate-out satisfied states from DB
        for phase in &mut phases {
            for (idx, gate) in phase.gates_out.iter_mut().enumerate() {
                if let Some(&satisfied) = gate_states.get(&(phase.id, idx as u32)) {
                    gate.satisfied = satisfied;
                }
            }
            // Compute word counts for this phase
            phase.words = manifest.counts_for_phase(phase.id);

            // Auto-compute phase status based on ARM64 tests + word counts + gates
            let all_gates_satisfied = phase.gates_out.iter().all(|g| g.satisfied);
            let any_gates_satisfied = phase.gates_out.iter().any(|g| g.satisfied);
            let phase_tests_passing = arm64_completed_phases.contains(&phase.id);

            if phase_tests_passing || (all_gates_satisfied && !phase.gates_out.is_empty()) {
                phase.status = PhaseStatus::Done;
            } else if any_gates_satisfied || phase.words.done > 0 {
                phase.status = PhaseStatus::Active;
            }
        }

        serde_json::to_value(phases).unwrap_or_default()
    }

    /// Detect which phases are complete based on ARM64 test results
    /// NOTE: Returns empty set to avoid blocking UI. Use refresh_adversarial_tests to update.
    fn detect_arm64_completed_phases(&self) -> std::collections::HashSet<u8> {
        // DISABLED: Running shell scripts synchronously freezes the UI.
        // Phase completion detection should be done async or cached.
        std::collections::HashSet::new()

        /*
        use std::collections::{HashMap, HashSet};
        use std::process::Command;

        let test_script = self.project_root.join("tools/arm64-tests/run-tests.sh");
        if !test_script.exists() {
            return HashSet::new();
        }

        let output = match Command::new(&test_script)
            .current_dir(&self.project_root)
            .output()
        {
            Ok(o) => o,
            Err(_) => return HashSet::new(),
        };

        let stdout = String::from_utf8_lossy(&output.stdout);

        // Parse test results: "PASS phaseN-xxx" or "FAIL phaseN-xxx"
        let mut phase_results: HashMap<u8, (u32, u32)> = HashMap::new(); // phase -> (pass, fail)

        for line in stdout.lines() {
            let (passed, name) = if line.starts_with("PASS ") {
                (true, &line[5..])
            } else if line.starts_with("FAIL ") {
                (false, &line[5..])
            } else {
                continue;
            };

            // Extract phase number from "phaseN-xxx"
            if let Some(rest) = name.trim().strip_prefix("phase") {
                if let Some(dash_pos) = rest.find('-') {
                    if let Ok(phase_num) = rest[..dash_pos].parse::<u8>() {
                        let entry = phase_results.entry(phase_num).or_insert((0, 0));
                        if passed {
                            entry.0 += 1;
                        } else {
                            entry.1 += 1;
                        }
                    }
                }
            }
        }

        // A phase is complete if it has at least one test and all tests pass
        phase_results
            .into_iter()
            .filter(|(_, (pass, fail))| *pass > 0 && *fail == 0)
            .map(|(phase, _)| phase)
            .collect()
        */
    }

    /// Toggle a gate check
    pub fn toggle_gate(&self, phase_id: u8, gate_idx: u32, satisfied: bool) -> Result<(), String> {
        let db = self.db.lock().unwrap();
        db.set_gate(phase_id, gate_idx, satisfied)
            .map_err(|e| e.to_string())
    }

    /// Set a manual word status override
    pub fn set_word_status(&self, word: &str, status: &WordStatus) -> Result<(), String> {
        let db = self.db.lock().unwrap();
        db.set_word_override(word, status)
            .map_err(|e| e.to_string())?;
        drop(db);
        self.rescan();
        Ok(())
    }

    /// Run the test suite
    pub fn run_tests(&self) -> Result<TestSummary, String> {
        let summary = runner::run_tests(&self.project_root)?;
        // Save to history
        let db = self.db.lock().unwrap();
        let _ = db.save_test_run(&summary);
        Ok(summary)
    }

    /// Get latest test results (most recent run from DB)
    pub fn get_latest_test_results(&self) -> Option<TestSummary> {
        let db = self.db.lock().unwrap();
        let history = db.get_test_history(1);
        history.first().map(|run| TestSummary {
            total: run.total,
            pass: run.pass,
            wrong: run.wrong,
            cfail: run.cfail,
            rfail: run.rfail,
            wall_ms: run.wall_ms,
            compile_ms: run.compile_ms,
            run_ms: run.run_ms,
            results: vec![],
        })
    }

    /// Get test run history
    pub fn get_test_history(&self, limit: u32) -> Vec<TestRun> {
        let db = self.db.lock().unwrap();
        db.get_test_history(limit)
    }
}

/// Build the phase definitions matching actual ARM64 development phases.
/// Phase numbers match ARM64 test naming (phase1-*, phase2-*, etc.)
fn build_phases() -> Vec<Phase> {
    vec![
        Phase {
            id: 1, name: "Exit + Assembler".into(), desc: "Basic exit(42), asm.fs instruction encoders".into(),
            file: "arch/arm64/asm.fs".into(), status: PhaseStatus::Pending,
            gates_in: vec![
                gate("C reference binary (tools/macho-test) runs on macOS ARM64"),
                gate("C engine (engine/fifth) builds and runs on ARM64"),
            ],
            gates_out: vec![
                gate("exit(42) works via Mach-O + SVC"),
                gate("All ARM64 instruction encoders verified"),
            ],
            deliverables: vec!["arch/arm64/asm.fs".into(), "arch/arm64/macho.fs".into()],
            risk: "".into(),
            words: Default::default(),
        },
        Phase {
            id: 2, name: "Literals".into(), desc: "emit-lit pushes values to TOS".into(),
            file: "arch/arm64/stack.fs".into(), status: PhaseStatus::Pending,
            gates_in: vec![gate("Phase 1 exit + asm working")],
            gates_out: vec![
                gate("emit-lit 42 produces exit code 42"),
                gate("Negative literals work"),
            ],
            deliverables: vec!["arch/arm64/stack.fs (emit-lit)".into()],
            risk: "".into(),
            words: Default::default(),
        },
        Phase {
            id: 3, name: "Arithmetic".into(), desc: "+, -, *, /, mod".into(),
            file: "arch/arm64/prims.fs".into(), status: PhaseStatus::Pending,
            gates_in: vec![gate("Phase 2 literals working")],
            gates_out: vec![
                gate("35 + 7 = 42"),
                gate("49 - 7 = 42"),
                gate("6 * 7 = 42"),
                gate("84 / 2 = 42"),
                gate("47 mod 5 = 2"),
            ],
            deliverables: vec!["arch/arm64/prims.fs (arithmetic)".into()],
            risk: "".into(),
            words: Default::default(),
        },
        Phase {
            id: 4, name: "Stack + Comparisons".into(), desc: "dup, swap, over, =, <".into(),
            file: "arch/arm64/stack.fs + prims.fs".into(), status: PhaseStatus::Pending,
            gates_in: vec![gate("Phase 3 arithmetic working")],
            gates_out: vec![
                gate("dup works"),
                gate("swap works"),
                gate("over works"),
                gate("= returns -1 for equal"),
                gate("< returns -1 for less than"),
            ],
            deliverables: vec!["arch/arm64/stack.fs".into(), "arch/arm64/prims.fs (comparisons)".into()],
            risk: "".into(),
            words: Default::default(),
        },
        Phase {
            id: 5, name: "Bitwise + Shifts".into(), desc: "and or xor invert lshift rshift".into(),
            file: "arch/arm64/prims.fs".into(), status: PhaseStatus::Pending,
            gates_in: vec![gate("Phase 4 stack + comparisons working")],
            gates_out: vec![
                gate("and works"),
                gate("or works"),
                gate("xor works"),
                gate("invert works"),
                gate("lshift works"),
                gate("rshift works"),
            ],
            deliverables: vec!["Bitwise ops in prims.fs".into()],
            risk: "".into(),
            words: Default::default(),
        },
        Phase {
            id: 6, name: "Control Flow".into(), desc: "if/then/else, begin/until/while".into(),
            file: "arch/arm64/control.fs".into(), status: PhaseStatus::Pending,
            gates_in: vec![gate("Phase 4 stack + comparisons working")],
            gates_out: vec![
                gate("if-then works"),
                gate("if-else-then works"),
                gate("begin-until works"),
                gate("begin-while-repeat works"),
            ],
            deliverables: vec!["arch/arm64/control.fs".into()],
            risk: "Branch offset encoding in instruction words".into(),
            words: Default::default(),
        },
        Phase {
            id: 7, name: "Function Calls".into(), desc: "Multi-word definitions, nested calls".into(),
            file: "arch/arm64/compile.fs".into(), status: PhaseStatus::Pending,
            gates_in: vec![gate("Phase 6 control flow working")],
            gates_out: vec![
                gate("Simple word call works"),
                gate("Chained calls work"),
                gate("Deep nesting works"),
            ],
            deliverables: vec!["arch/arm64/compile.fs (gen-call, gen-ret)".into()],
            risk: "Return stack management".into(),
            words: Default::default(),
        },
        Phase {
            id: 8, name: "I/O".into(), desc: "emit, cr, .\" strings, type".into(),
            file: "arch/arm64/stack.fs (I/O section)".into(), status: PhaseStatus::Pending,
            gates_in: vec![gate("Phase 7 function calls working")],
            gates_out: vec![
                gate("cr prints newline"),
                gate(".\" string\" prints string"),
                gate("Hello world works"),
            ],
            deliverables: vec!["I/O primitives in stack.fs".into()],
            risk: "".into(),
            words: Default::default(),
        },
        Phase {
            id: 9, name: "Return Stack".into(), desc: ">r r> r@".into(),
            file: "arch/arm64/stack.fs".into(), status: PhaseStatus::Pending,
            gates_in: vec![gate("Phase 8 I/O working")],
            gates_out: vec![
                gate(">r pushes to return stack"),
                gate("r> pops from return stack"),
                gate("r@ peeks return stack"),
            ],
            deliverables: vec!["Return stack ops in stack.fs".into()],
            risk: "".into(),
            words: Default::default(),
        },
        Phase {
            id: 10, name: "Memory Operations".into(), desc: "@, !, c@, c!, +!".into(),
            file: "arch/arm64/prims.fs".into(), status: PhaseStatus::Pending,
            gates_in: vec![gate("Phase 8 I/O working")],
            gates_out: vec![
                gate("@ fetches 64-bit value"),
                gate("! stores 64-bit value"),
                gate("c@ fetches byte"),
                gate("+! increments memory"),
            ],
            deliverables: vec!["Memory primitives in prims.fs".into()],
            risk: "Address alignment requirements".into(),
            words: Default::default(),
        },
        Phase {
            id: 11, name: "Do/loop".into(), desc: "do loop +loop i j leave unloop".into(),
            file: "arch/arm64/control.fs".into(), status: PhaseStatus::Pending,
            gates_in: vec![gate("Phase 10 memory operations working")],
            gates_out: vec![
                gate("do/loop counts correctly"),
                gate("I returns loop index"),
                gate("+loop with increment works"),
                gate("leave exits loop early"),
            ],
            deliverables: vec!["do/loop in control.fs".into()],
            risk: "Return stack usage for loop indices".into(),
            words: Default::default(),
        },
        Phase {
            id: 12, name: "Variables/constants".into(), desc: "variable constant create".into(),
            file: "arch/arm64/compile.fs".into(), status: PhaseStatus::Pending,
            gates_in: vec![gate("Phase 11 do/loop working")],
            gates_out: vec![
                gate("variable creates storage"),
                gate("constant returns value"),
                gate("create makes dictionary entry"),
            ],
            deliverables: vec!["Variable support".into()],
            risk: "Data segment allocation".into(),
            words: Default::default(),
        },
        Phase {
            id: 13, name: "Forward references".into(), desc: "Words calling words defined later".into(),
            file: "arch/arm64/compile.fs".into(), status: PhaseStatus::Pending,
            gates_in: vec![gate("Phase 12 variables working")],
            gates_out: vec![
                gate("Forward calls resolve correctly"),
                gate("Mutual recursion works"),
            ],
            deliverables: vec!["Forward reference resolution".into()],
            risk: "Two-pass or patching required".into(),
            words: Default::default(),
        },
        Phase {
            id: 14, name: "Exit".into(), desc: "Early return from word".into(),
            file: "arch/arm64/control.fs".into(), status: PhaseStatus::Pending,
            gates_in: vec![gate("Phase 13 forward refs working")],
            gates_out: vec![
                gate("exit returns from word immediately"),
                gate("Works inside nested control flow"),
            ],
            deliverables: vec!["exit in control.fs".into()],
            risk: "Stack cleanup on early exit".into(),
            words: Default::default(),
        },
        Phase {
            id: 15, name: "Test Suite".into(), desc: "All compiler tests passing".into(),
            file: "".into(), status: PhaseStatus::Pending,
            gates_in: vec![gate("Phase 14 exit working")],
            gates_out: vec![
                gate("All 44+ compiler tests pass"),
                gate("Hayes ANS tests pass"),
                gate("Adversarial tests pass"),
            ],
            deliverables: vec!["Full test suite green".into()],
            risk: "".into(),
            words: Default::default(),
        },
        Phase {
            id: 16, name: "Self-Hosting".into(), desc: "Shannon compiles Shannon".into(),
            file: "".into(), status: PhaseStatus::Pending,
            gates_in: vec![gate("Phase 15 test suite passing")],
            gates_out: vec![
                gate("Shannon compiles itself to ARM64"),
                gate("Self-compiled matches C-compiled output"),
                gate("C engine can be deleted"),
            ],
            deliverables: vec!["Self-hosted ARM64 binary".into()],
            risk: "".into(),
            words: Default::default(),
        },
    ]
}

fn gate(text: &str) -> Gate {
    Gate {
        text: text.to_string(),
        satisfied: false,
        auto_computed: false,
    }
}

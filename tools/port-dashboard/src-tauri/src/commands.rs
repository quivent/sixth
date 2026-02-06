use tauri::State;

use crate::models::{FileInfo, TestCategoryGroup, TestResult, TestRun, TestSummary, Word, WordStatus};
use crate::parser;
use crate::runner;
use crate::state::AppState;

#[tauri::command]
pub fn get_word_data(state: State<AppState>) -> Vec<Word> {
    state.get_word_data()
}

#[tauri::command]
pub fn get_phases(state: State<AppState>) -> serde_json::Value {
    state.get_phases()
}

#[tauri::command]
pub fn get_test_results(state: State<AppState>) -> Option<TestSummary> {
    state.get_latest_test_results()
}

#[tauri::command]
pub fn run_tests(state: State<AppState>) -> Result<TestSummary, String> {
    state.run_tests()
}

/// Refresh adversarial test cache. Call this to update per-word pass/fail status.
/// Takes ~1s to run all 34 adversarial tests.
#[tauri::command]
pub fn refresh_adversarial_tests(state: State<AppState>) {
    state.refresh_tests();
    state.rescan(); // Re-apply cached results to words
}

/// Get adversarial test summary from cache.
#[tauri::command]
pub fn get_adversarial_summary(state: State<AppState>) -> serde_json::Value {
    let (total, passing) = state.get_adversarial_summary();
    serde_json::json!({
        "total": total,
        "pass": passing,
        "fail": total - passing
    })
}

#[tauri::command]
pub fn toggle_gate(phase_id: u8, gate_idx: u32, satisfied: bool, state: State<AppState>) -> Result<(), String> {
    state.toggle_gate(phase_id, gate_idx, satisfied)
}

#[tauri::command]
pub fn set_word_status(word: String, status: String, state: State<AppState>) -> Result<(), String> {
    let ws = WordStatus::from_str(&status);
    state.set_word_status(&word, &ws)
}

#[tauri::command]
pub fn get_test_history(state: State<AppState>) -> Vec<TestRun> {
    state.get_test_history(50)
}

#[tauri::command]
pub fn get_project_root(state: State<AppState>) -> String {
    state.project_root().to_string_lossy().to_string()
}

/// Scan the actual filesystem and return live file inventory.
/// This is the single source of truth — no hardcoded data.
#[tauri::command]
pub fn get_file_inventory(state: State<AppState>) -> Vec<FileInfo> {
    let root = state.project_root();
    let shannon = root.join("compiler/shannon");
    let arm64_dir = shannon.join("arch/arm64");
    let x86_dir = shannon.join("arch/x86");

    // x86 reference data (phase mapping)
    let x86_phase_map: Vec<(&str, u8, &str)> = vec![
        ("asm.fs", 1, "ARM64 assembler: instruction encoding"),
        ("elf.fs", 2, "Binary format (Mach-O on ARM64)"),
        ("stack.fs", 3, "Data stack operations with register caching"),
        ("prims.fs", 4, "Primitive word code generation"),
        ("control.fs", 5, "Control flow: if/then/else, loops"),
        ("io.fs", 6, "I/O primitives via syscalls"),
        ("rstack.fs", 7, "Return stack operations"),
    ];

    // ARM64 file name mapping (arm64 name -> x86 equivalent)
    let arm64_to_x86: Vec<(&str, &str)> = vec![
        ("asm.fs", "asm.fs"),
        ("macho.fs", "elf.fs"),
        ("stack.fs", "stack.fs"),
        ("prims.fs", "prims.fs"),
        ("control.fs", "control.fs"),
        ("io.fs", "io.fs"),
        ("rstack.fs", "rstack.fs"),
    ];

    let mut files = Vec::new();

    // Scan x86 reference files
    for (name, phase, _desc) in &x86_phase_map {
        let path = x86_dir.join(name);
        let words = parser::scan_words(&path);
        let lines = std::fs::read_to_string(&path)
            .map(|c| c.lines().count() as u32)
            .unwrap_or(0);
        let word_defs = words.iter().filter(|_| true).count() as u32; // all are word defs
        // Count specifically colon defs vs constants
        let content = std::fs::read_to_string(&path).unwrap_or_default();
        let colon_count = regex::Regex::new(r"(?m)^:\s+\S+")
            .unwrap()
            .find_iter(&content)
            .count() as u32;
        let const_count = word_defs.saturating_sub(colon_count);

        files.push(FileInfo {
            name: format!("arch/x86/{}", name),
            category: "arch-x86".into(),
            phase: *phase,
            lines,
            word_defs: colon_count,
            constants: const_count,
            x86_ref: String::new(),
            x86_lines: 0,
            x86_words: 0,
            words,
            status: "reference".into(),
        });
    }

    // Scan ARM64 files (both existing and expected)
    for (arm64_name, x86_name) in &arm64_to_x86 {
        let arm64_path = arm64_dir.join(arm64_name);
        let x86_path = x86_dir.join(x86_name);

        // Get x86 reference stats
        let x86_content = std::fs::read_to_string(&x86_path).unwrap_or_default();
        let x86_lines = x86_content.lines().count() as u32;
        let x86_words = regex::Regex::new(r"(?m)^:\s+\S+")
            .unwrap()
            .find_iter(&x86_content)
            .count() as u32;

        // Find phase for this file
        let phase = x86_phase_map
            .iter()
            .find(|(n, _, _)| *n == *x86_name)
            .map(|(_, p, _)| *p)
            .unwrap_or(0);

        if arm64_path.exists() {
            let words = parser::scan_words(&arm64_path);
            let content = std::fs::read_to_string(&arm64_path).unwrap_or_default();
            let lines = content.lines().count() as u32;
            let colon_count = regex::Regex::new(r"(?m)^:\s+\S+")
                .unwrap()
                .find_iter(&content)
                .count() as u32;
            let const_count = regex::Regex::new(r"(?m)^\s*\-?\d+\s+constant\s+\S+")
                .unwrap()
                .find_iter(&content)
                .count() as u32;

            // Determine status based on content
            let status = if colon_count >= x86_words / 2 {
                "active"
            } else if colon_count > 0 {
                "stub"
            } else {
                "stub"
            };

            files.push(FileInfo {
                name: format!("arch/arm64/{}", arm64_name),
                category: "arch-arm64".into(),
                phase,
                lines,
                word_defs: colon_count,
                constants: const_count,
                x86_ref: format!("arch/x86/{}", x86_name),
                x86_lines,
                x86_words,
                words,
                status: status.into(),
            });
        } else {
            files.push(FileInfo {
                name: format!("arch/arm64/{}", arm64_name),
                category: "arch-arm64".into(),
                phase,
                lines: 0,
                word_defs: 0,
                constants: 0,
                x86_ref: format!("arch/x86/{}", x86_name),
                x86_lines,
                x86_words,
                words: vec![],
                status: "pending".into(),
            });
        }
    }

    // Scan shared files
    let shared_files = vec![
        ("defs.fs", "Shared definitions - needs conditional register constants"),
        ("strings.fs", "String handling - may need alignment adjustments"),
        ("compile.fs", "Compiler core - needs arch dispatch"),
        ("main.fs", "Main entry - needs ARM64 include path"),
    ];
    for (name, _desc) in &shared_files {
        let path = shannon.join(name);
        let lines = std::fs::read_to_string(&path)
            .map(|c| c.lines().count() as u32)
            .unwrap_or(0);
        let words = parser::scan_words(&path);
        files.push(FileInfo {
            name: name.to_string(),
            category: "shared".into(),
            phase: 7,
            lines,
            word_defs: words.len() as u32,
            constants: 0,
            x86_ref: String::new(),
            x86_lines: 0,
            x86_words: 0,
            words,
            status: "patch".into(),
        });
    }

    // Unchanged files
    let unchanged = vec!["scan.fs", "dispatch.fs", "opt-fold.fs", "opt-fuse.fs", "opt-swap.fs"];
    for name in &unchanged {
        let path = shannon.join(name);
        let lines = std::fs::read_to_string(&path)
            .map(|c| c.lines().count() as u32)
            .unwrap_or(0);
        files.push(FileInfo {
            name: name.to_string(),
            category: "unchanged".into(),
            phase: 0,
            lines,
            word_defs: 0,
            constants: 0,
            x86_ref: String::new(),
            x86_lines: 0,
            x86_words: 0,
            words: vec![],
            status: "unchanged".into(),
        });
    }

    files
}

/// Read a source file relative to compiler/shannon/ directory.
/// Returns numbered lines for display in the code viewer.
#[tauri::command]
pub fn read_source_file(path: String, state: State<AppState>) -> Result<String, String> {
    // Sanitize: only allow paths under compiler/shannon/
    let clean = path.replace("..", "").replace("//", "/");
    let full_path = state.project_root().join("compiler/shannon").join(&clean);

    if !full_path.exists() {
        return Err(format!("File not found: {}", clean));
    }
    if !full_path.starts_with(state.project_root().join("compiler/shannon")) {
        return Err("Access denied: path must be under compiler/shannon/".into());
    }

    std::fs::read_to_string(&full_path)
        .map_err(|e| format!("Read error: {}", e))
}

/// Scan all test files and return them grouped by category.
/// Does NOT run tests - just discovers what exists.
#[tauri::command]
pub fn scan_all_tests(state: State<AppState>) -> Vec<TestCategoryGroup> {
    let result = runner::scan_all_tests(state.project_root());
    eprintln!("[scan_all_tests] Found {} categories with {} total tests",
        result.len(),
        result.iter().map(|g| g.tests.len()).sum::<usize>());
    result
}

/// Run all tests and return results grouped by category.
/// This runs the full test suite (~1660 tests) - may take a few seconds.
#[tauri::command]
pub fn run_all_tests(state: State<AppState>) -> Result<Vec<TestCategoryGroup>, String> {
    runner::run_all_tests_grouped(state.project_root())
}

/// Run a single test by name and return the result.
#[tauri::command]
pub fn run_single_test(name: String, state: State<AppState>) -> Result<TestResult, String> {
    runner::run_single_test(state.project_root(), &name)
}

/// Read a test file from compiler/tests/ directory.
#[tauri::command]
pub fn read_test_file(name: String, state: State<AppState>) -> Result<String, String> {
    let clean = name.replace("..", "").replace("//", "/");
    let full_path = state.project_root().join("compiler/tests").join(format!("{}.fs", clean));

    if !full_path.exists() {
        return Err(format!("Test file not found: {}", clean));
    }

    std::fs::read_to_string(&full_path)
        .map_err(|e| format!("Read error: {}", e))
}

/// Get ARM64 port status summary from tools/arm64-tests/
#[tauri::command]
pub fn get_arm64_status(state: State<AppState>) -> serde_json::Value {
    use std::process::Command;

    let root = state.project_root();
    let test_script = root.join("tools/arm64-tests/run-tests.sh");

    // Check if ARM64 tests exist
    if !test_script.exists() {
        return serde_json::json!({
            "available": false,
            "total": 0,
            "pass": 0,
            "fail": 0,
            "tests": []
        });
    }

    // Run the ARM64 tests
    let output = match Command::new(&test_script)
        .current_dir(&root)
        .output() {
            Ok(o) => o,
            Err(_) => return serde_json::json!({
                "available": true,
                "total": 0,
                "pass": 0,
                "fail": 0,
                "error": "Failed to run tests",
                "tests": []
            }),
        };

    let stdout = String::from_utf8_lossy(&output.stdout).to_string();

    // Parse output: "PASS name" or "FAIL name" lines, and "TOTAL: N  PASS: N  FAIL: N"
    let mut tests = Vec::new();
    let mut total = 0u32;
    let mut pass = 0u32;
    let mut fail = 0u32;

    for line in stdout.lines() {
        if line.starts_with("PASS ") {
            tests.push(serde_json::json!({
                "name": line[5..].trim(),
                "status": "pass"
            }));
        } else if line.starts_with("FAIL ") {
            tests.push(serde_json::json!({
                "name": line[5..].trim(),
                "status": "fail"
            }));
        } else if line.starts_with("TOTAL:") {
            // Parse "TOTAL: 22  PASS: 22  FAIL: 0"
            let re = regex::Regex::new(r"TOTAL:\s*(\d+)\s+PASS:\s*(\d+)\s+FAIL:\s*(\d+)").unwrap();
            if let Some(cap) = re.captures(line) {
                total = cap[1].parse().unwrap_or(0);
                pass = cap[2].parse().unwrap_or(0);
                fail = cap[3].parse().unwrap_or(0);
            }
        }
    }

    serde_json::json!({
        "available": true,
        "total": total,
        "pass": pass,
        "fail": fail,
        "tests": tests
    })
}

/// Get ARM64 test results grouped by phase
/// Returns: { phase_id: { pass: N, fail: N, tests: [{name, pass}] } }
#[tauri::command]
pub fn get_arm64_test_results(state: State<AppState>) -> serde_json::Value {
    use std::collections::HashMap;
    use std::process::Command;

    let root = state.project_root();
    let test_script = root.join("tools/arm64-tests/run-tests.sh");

    if !test_script.exists() {
        return serde_json::json!({});
    }

    let output = match Command::new(&test_script)
        .current_dir(&root)
        .output() {
            Ok(o) => o,
            Err(_) => return serde_json::json!({}),
        };

    let stdout = String::from_utf8_lossy(&output.stdout);

    // Group results by phase
    let mut phases: HashMap<u8, serde_json::Value> = HashMap::new();

    for line in stdout.lines() {
        let (passed, name) = if line.starts_with("PASS ") {
            (true, &line[5..])
        } else if line.starts_with("FAIL ") {
            (false, &line[5..])
        } else {
            continue;
        };

        let name = name.trim();

        // Extract phase number from "phaseN-xxx"
        if let Some(rest) = name.strip_prefix("phase") {
            if let Some(dash_pos) = rest.find('-') {
                if let Ok(phase_num) = rest[..dash_pos].parse::<u8>() {
                    let test_name = &rest[dash_pos + 1..];

                    let entry = phases.entry(phase_num).or_insert_with(|| {
                        serde_json::json!({
                            "pass": 0,
                            "fail": 0,
                            "tests": []
                        })
                    });

                    if passed {
                        if let Some(p) = entry.get_mut("pass") {
                            *p = serde_json::json!(p.as_u64().unwrap_or(0) + 1);
                        }
                    } else {
                        if let Some(f) = entry.get_mut("fail") {
                            *f = serde_json::json!(f.as_u64().unwrap_or(0) + 1);
                        }
                    }

                    if let Some(tests) = entry.get_mut("tests") {
                        if let Some(arr) = tests.as_array_mut() {
                            arr.push(serde_json::json!({
                                "name": test_name,
                                "pass": passed
                            }));
                        }
                    }
                }
            }
        }
    }

    serde_json::to_value(phases).unwrap_or_default()
}

/// Run ARM64 tests and return results
#[tauri::command]
pub fn run_arm64_tests(state: State<AppState>) -> serde_json::Value {
    // Just re-run and return the grouped results
    get_arm64_test_results(state)
}

/// Run interactive Forth code through the compiler.
/// Returns: { "compiled": bool, "output": String, "exit_code": i32 }
#[tauri::command]
pub fn run_interactive_test(code: String, state: State<AppState>) -> Result<serde_json::Value, String> {
    use std::process::Command;
    use std::io::Write;

    let root = state.project_root();
    let temp_dir = std::env::temp_dir();
    let test_file = temp_dir.join("dashboard-test.fs");
    let binary_path = temp_dir.join("dashboard-test");

    // Write the test code to a temp file
    let mut file = std::fs::File::create(&test_file)
        .map_err(|e| format!("Failed to create temp file: {}", e))?;
    file.write_all(code.as_bytes())
        .map_err(|e| format!("Failed to write test code: {}", e))?;

    // Compile with Shannon compiler
    let compile_output = Command::new(root.join("engine/fifth"))
        .current_dir(&root)
        .arg(root.join("compiler/shannon/main.fs"))
        .arg(&test_file)
        .arg(&binary_path)
        .output()
        .map_err(|e| format!("Failed to run compiler: {}", e))?;

    let compile_stderr = String::from_utf8_lossy(&compile_output.stderr).to_string();
    let compile_stdout = String::from_utf8_lossy(&compile_output.stdout).to_string();

    if !compile_output.status.success() {
        return Ok(serde_json::json!({
            "compiled": false,
            "compile_output": format!("{}\n{}", compile_stdout, compile_stderr).trim(),
            "output": "",
            "exit_code": compile_output.status.code().unwrap_or(-1)
        }));
    }

    // Sign the binary (macOS requirement)
    let _ = Command::new("codesign")
        .args(["-f", "-s", "-"])
        .arg(&binary_path)
        .output();

    // Run the compiled binary
    let run_output = Command::new(&binary_path)
        .current_dir(&root)
        .output()
        .map_err(|e| format!("Failed to run binary: {}", e))?;

    let stdout = String::from_utf8_lossy(&run_output.stdout).to_string();
    let stderr = String::from_utf8_lossy(&run_output.stderr).to_string();
    let exit_code = run_output.status.code().unwrap_or(-1);

    // Clean up
    let _ = std::fs::remove_file(&test_file);
    let _ = std::fs::remove_file(&binary_path);

    Ok(serde_json::json!({
        "compiled": true,
        "compile_output": format!("{}\n{}", compile_stdout, compile_stderr).trim(),
        "output": format!("{}{}", stdout, stderr),
        "exit_code": exit_code
    }))
}

// ============================================================
// BENCHMARK COMMANDS
// ============================================================

use crate::benchmark;
use crate::models::{BenchmarkDef, BenchmarkSummary};

/// Get list of available benchmarks
#[tauri::command]
pub fn get_benchmarks() -> Vec<BenchmarkDef> {
    benchmark::get_benchmark_definitions()
}

/// Run all benchmarks and return summary
#[tauri::command]
pub fn run_benchmarks(state: State<AppState>) -> BenchmarkSummary {
    benchmark::run_all_benchmarks(state.project_root())
}

/// Run a single benchmark by name (quick mode - 3 iterations for responsiveness)
#[tauri::command]
pub fn run_benchmark(name: String, state: State<AppState>) -> Option<crate::models::BenchmarkResult> {
    println!("[benchmark] Running single benchmark: {}", name);
    let definitions = benchmark::get_benchmark_definitions();
    let def = definitions.iter().find(|d| d.name == name)?;

    // Prepare GCC binaries (compile bench.c at all optimization levels)
    let gcc_binaries = match benchmark::prepare_gcc_binaries(state.project_root()) {
        Ok(bins) => bins,
        Err(e) => {
            println!("[benchmark] Failed to compile GCC: {}", e);
            return None;
        }
    };

    let temp_dir = std::env::temp_dir().join("sixth-benchmarks");
    let _ = std::fs::create_dir_all(&temp_dir);

    // Use 3 runs for quick single-benchmark testing (vs 10 for full suite)
    let result = benchmark::run_benchmark(state.project_root(), def, &temp_dir, &gcc_binaries, 3);
    println!("[benchmark] Done: {}", name);
    Some(result)
}

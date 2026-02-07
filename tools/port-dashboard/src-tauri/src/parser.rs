use regex::Regex;
use std::collections::{HashMap, HashSet};
use std::fs;
use std::path::Path;

use crate::models::{Word, WordCounts, WordStatus};

/// Scan a .fs file and return all word names defined in it.
/// Matches `: word-name` and `N constant name` patterns.
pub fn scan_words(path: &Path) -> Vec<String> {
    let content = match fs::read_to_string(path) {
        Ok(c) => c,
        Err(_) => return vec![],
    };

    let colon_re = Regex::new(r"(?m)^:\s+(\S+)").unwrap();
    let const_re = Regex::new(r"(?m)^\s*\-?\d+\s+constant\s+(\S+)").unwrap();
    let variable_re = Regex::new(r"(?m)^\s*variable\s+(\S+)").unwrap();
    let create_re = Regex::new(r"(?m)^\s*create\s+(\S+)").unwrap();

    let mut words = Vec::new();

    for cap in colon_re.captures_iter(&content) {
        words.push(cap[1].to_string());
    }
    for cap in const_re.captures_iter(&content) {
        words.push(cap[1].to_string());
    }
    for cap in variable_re.captures_iter(&content) {
        words.push(cap[1].to_string());
    }
    for cap in create_re.captures_iter(&content) {
        words.push(cap[1].to_string());
    }

    words
}

/// Scan all .fs files in a directory recursively.
/// Returns a map of filename -> set of word names.
pub fn scan_directory(dir: &Path) -> HashMap<String, HashSet<String>> {
    let mut result = HashMap::new();

    if !dir.exists() {
        return result;
    }

    let entries = match fs::read_dir(dir) {
        Ok(e) => e,
        Err(_) => return result,
    };

    for entry in entries.flatten() {
        let path = entry.path();
        if path.is_file() && path.extension().map_or(false, |e| e == "fs") {
            let filename = path.file_name().unwrap().to_string_lossy().to_string();
            let words: HashSet<String> = scan_words(&path).into_iter().collect();
            result.insert(filename, words);
        } else if path.is_dir() {
            // Recurse into subdirectories
            let sub = scan_directory(&path);
            for (k, v) in sub {
                result.insert(k, v);
            }
        }
    }

    result
}

/// Scan adversarial test files and return a map of word -> test files that cover it.
/// Extracts words used in test function bodies.
pub fn scan_adversarial_tests(dir: &Path) -> HashMap<String, Vec<String>> {
    let mut word_to_tests: HashMap<String, Vec<String>> = HashMap::new();

    if !dir.exists() {
        return word_to_tests;
    }

    let entries = match fs::read_dir(dir) {
        Ok(e) => e,
        Err(_) => return word_to_tests,
    };

    // Common Forth words to look for in test files
    // These are the primitives and core words we care about
    let target_words: HashSet<&str> = [
        // Stack ops
        "dup", "drop", "swap", "over", "rot", "-rot", "nip", "tuck", "pick", "roll",
        "2dup", "2drop", "2swap", "2over", "2rot", "?dup",
        // Arithmetic
        "+", "-", "*", "/", "mod", "/mod", "*/", "*/mod", "negate", "abs",
        "1+", "1-", "2+", "2-", "2*", "2/",
        // Comparison
        "=", "<>", "<", ">", "<=", ">=", "0=", "0<", "0>", "0<>",
        "u<", "u>", "min", "max", "within",
        // Logic
        "and", "or", "xor", "invert", "lshift", "rshift",
        // Memory
        "@", "!", "+!", "c@", "c!", "2@", "2!", "fill", "move", "cmove",
        // Return stack
        ">r", "r>", "r@", "2>r", "2r>", "2r@",
        // Control flow
        "if", "then", "else", "begin", "until", "again", "while", "repeat",
        "do", "loop", "+loop", "i", "j", "leave", "unloop", "exit",
        "case", "of", "endof", "endcase",
        // Double cell
        "s>d", "d>s", "d+", "d-", "dnegate", "dabs", "d=", "d<", "d0=", "d0<",
        "m+", "m*", "um*", "um/mod", "fm/mod", "sm/rem",
        // Strings
        "count", "type", "emit", "cr", "space", "spaces",
        // Variables
        "variable", "constant", "create", "does>", "value", "to",
        // Misc
        "execute", "recurse", "defer", "is", "action-of",
    ].iter().cloned().collect();

    for entry in entries.flatten() {
        let path = entry.path();
        if path.is_file() && path.extension().map_or(false, |e| e == "fs") {
            let filename = path.file_name().unwrap().to_string_lossy().to_string();

            // Only process numbered adversarial tests (0001-xxx.fs format)
            if !filename.chars().next().map_or(false, |c| c.is_ascii_digit()) {
                continue;
            }

            let content = match fs::read_to_string(&path) {
                Ok(c) => c,
                Err(_) => continue,
            };

            // Extract test name (e.g., "0001-stack-basic" from filename)
            let test_name = filename.trim_end_matches(".fs").to_string();

            // Find all words used in this test file
            // Simple tokenization: split on whitespace, check against target words
            let tokens: HashSet<&str> = content
                .split(|c: char| c.is_whitespace() || c == '(' || c == ')')
                .filter(|s| !s.is_empty())
                .collect();

            for word in &target_words {
                if tokens.contains(*word) {
                    word_to_tests
                        .entry(word.to_string())
                        .or_default()
                        .push(test_name.clone());
                }
            }
        }
    }

    // Sort test lists for consistent output
    for tests in word_to_tests.values_mut() {
        tests.sort();
        tests.dedup();
    }

    word_to_tests
}

/// The manifest of all words that need porting, with their baseline metadata.
/// This is the source of truth — derived from the WORD_DATA that was in the HTML.
/// In a real deployment, this would be loaded from a JSON file. For now, we
/// embed a simplified version that the parser enriches with live file data.
#[derive(Debug, Clone)]
pub struct WordManifest {
    pub words: Vec<Word>,
}

impl WordManifest {
    /// Load the word manifest from a JSON file
    pub fn load(path: &Path) -> Self {
        if path.exists() {
            if let Ok(content) = fs::read_to_string(path) {
                if let Ok(words) = serde_json::from_str::<Vec<Word>>(&content) {
                    return WordManifest { words };
                }
            }
        }
        // Fallback: empty manifest
        WordManifest { words: vec![] }
    }

    /// Save the manifest to JSON
    pub fn save(&self, path: &Path) -> std::io::Result<()> {
        let json = serde_json::to_string_pretty(&self.words)?;
        fs::write(path, json)
    }

    /// Update word statuses based on what's actually defined in ARM64 files.
    /// Words found in arch/arm64/*.fs get status "done".
    /// Manual overrides (from DB) take precedence.
    /// ARM64 words are the source of truth. Manifest provides metadata (phase, notes).
    pub fn update_from_scan(
        &mut self,
        arm64_words: &HashMap<String, HashSet<String>>,
        overrides: &HashMap<String, WordStatus>,
        adversarial_tests: &HashMap<String, Vec<String>>,
        test_results: &HashMap<String, bool>,
    ) {
        // Collect all ARM64 words with their source files
        let mut arm64_all: HashMap<String, String> = HashMap::new();
        for (file, words) in arm64_words {
            for word in words {
                arm64_all.insert(word.clone(), file.clone());
            }
        }

        // Build lookup from manifest for metadata (phase, notes)
        let manifest_meta: HashMap<String, (u8, String)> = self.words.iter()
            .map(|w| (w.word.clone(), (w.phase, w.note.clone())))
            .collect();

        // ARM64 words are the source of truth - rebuild word list from scratch
        let mut new_words = Vec::new();
        for (word_name, file) in &arm64_all {
            let (phase, note) = manifest_meta.get(word_name)
                .cloned()
                .unwrap_or((0, String::new()));

            let status = if let Some(s) = overrides.get(word_name) {
                s.clone()
            } else {
                WordStatus::Done // In ARM64, so it's done
            };

            let mut word = Word {
                word: word_name.clone(),
                phase,
                file: file.clone(),
                status,
                note,
                tests: vec![],
                tests_passing: 0,
                tests_failing: 0,
            };
            Self::update_word_tests(&mut word, adversarial_tests, test_results);
            new_words.push(word);
        }

        self.words = new_words;
    }

    fn update_word_tests(
        word: &mut Word,
        adversarial_tests: &HashMap<String, Vec<String>>,
        test_results: &HashMap<String, bool>,
    ) {
        if let Some(tests) = adversarial_tests.get(&word.word) {
            word.tests = tests.clone();
            let mut passing = 0u32;
            let mut failing = 0u32;
            for test_name in tests {
                if let Some(&passed) = test_results.get(test_name) {
                    if passed { passing += 1; } else { failing += 1; }
                }
            }
            word.tests_passing = passing;
            word.tests_failing = failing;
        } else {
            word.tests = vec![];
            word.tests_passing = 0;
            word.tests_failing = 0;
        }
    }

    /// Compute word counts by status
    pub fn counts(&self) -> WordCounts {
        let mut c = WordCounts::default();
        for w in &self.words {
            match w.status {
                WordStatus::Done => c.done += 1,
                WordStatus::Pending => c.pending += 1,
                WordStatus::Redesign => c.redesign += 1,
                WordStatus::Eliminated => c.eliminated += 1,
            }
        }
        c
    }

    /// Compute word counts for a specific phase
    pub fn counts_for_phase(&self, phase: u8) -> WordCounts {
        let mut c = WordCounts::default();
        for w in &self.words {
            if w.phase == phase {
                match w.status {
                    WordStatus::Done => c.done += 1,
                    WordStatus::Pending => c.pending += 1,
                    WordStatus::Redesign => c.redesign += 1,
                    WordStatus::Eliminated => c.eliminated += 1,
                }
            }
        }
        c
    }
}

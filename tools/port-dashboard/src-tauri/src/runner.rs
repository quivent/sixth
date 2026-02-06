use regex::Regex;
use std::collections::HashMap;
use std::fs;
use std::path::Path;
use std::process::Command;

use crate::models::{TestCategory, TestCategoryGroup, TestResult, TestStatus, TestSummary};

/// Run adversarial tests and return a map of test name -> pass/fail.
/// Test names are like "0001-stack-basic" (without .fs extension).
pub fn run_adversarial_tests(project_root: &Path) -> HashMap<String, bool> {
    let mut results = HashMap::new();

    let test_script = project_root.join("compiler/tests/test");
    if !test_script.exists() {
        return results;
    }

    // Run tests with pattern matching adversarial tests
    // Pattern is "adversarial/*.fs" to match test script expectations
    let output = Command::new(&test_script)
        .current_dir(project_root)
        .arg("adversarial/*.fs")
        .output();

    let output = match output {
        Ok(o) => o,
        Err(_) => return results,
    };

    let stdout = String::from_utf8_lossy(&output.stdout).to_string();
    let stderr = String::from_utf8_lossy(&output.stderr).to_string();
    let combined = format!("{}\n{}", stdout, stderr);

    // Parse individual results: "name:STATUS:compile_ms:run_ms"
    // Results show up in the summary list at the end
    let result_re = Regex::new(r"(?m)^\s*(\S+):(PASS|WRONG|CFAIL|RFAIL):(\d+):(\d+)").unwrap();

    for cap in result_re.captures_iter(&combined) {
        let name = cap[1].to_string();
        let passed = &cap[2] == "PASS";
        results.insert(name, passed);
    }

    // If no structured results found, try parsing the failure lists
    if results.is_empty() {
        // Parse "Runtime failures:" and "Compile failures:" sections
        let runtime_fail_re = Regex::new(r"Runtime failures:\n((?:\s+\S+\n)+)").unwrap();
        let compile_fail_re = Regex::new(r"Compile failures:\n((?:\s+\S+\n)+)").unwrap();
        let wrong_re = Regex::new(r"Wrong output:\n((?:\s+\S+\n)+)").unwrap();

        // Mark failures
        for re in [&runtime_fail_re, &compile_fail_re, &wrong_re] {
            if let Some(cap) = re.captures(&combined) {
                for line in cap[1].lines() {
                    let name = line.trim();
                    if !name.is_empty() {
                        results.insert(name.to_string(), false);
                    }
                }
            }
        }

        // Anything in the test count but not in failures is a pass
        // Parse "Running N tests..."
        if let Some(cap) = Regex::new(r"TOTAL:\s*(\d+)\s+PASS:\s*(\d+)").unwrap().captures(&combined) {
            let _total: u32 = cap[1].parse().unwrap_or(0);
            let pass_count: u32 = cap[2].parse().unwrap_or(0);
            // If we have passes but haven't found them, we can't determine which ones
            // Just note that some passed
            if pass_count > 0 && results.values().all(|&v| !v) {
                eprintln!("[runner] Warning: {} tests passed but couldn't identify which ones", pass_count);
            }
        }
    }

    results
}

/// Run the test suite and parse results.
/// Executes `compiler/tests/test` from the project root.
pub fn run_tests(project_root: &Path) -> Result<TestSummary, String> {
    let test_script = project_root.join("compiler/tests/test");
    if !test_script.exists() {
        return Err(format!("Test script not found: {}", test_script.display()));
    }

    let output = Command::new(&test_script)
        .current_dir(project_root)
        .env("VERBOSE", "1")
        .output()
        .map_err(|e| format!("Failed to run tests: {}", e))?;

    let stdout = String::from_utf8_lossy(&output.stdout).to_string();
    let stderr = String::from_utf8_lossy(&output.stderr).to_string();
    let combined = format!("{}\n{}", stdout, stderr);

    parse_test_output(&combined)
}

/// Parse the test runner output into a TestSummary.
pub fn parse_test_output(output: &str) -> Result<TestSummary, String> {
    let mut summary = TestSummary::default();

    // Parse summary line: "TOTAL: 44  PASS: 44  WRONG: 0  CFAIL: 0  RFAIL: 0"
    let summary_re =
        Regex::new(r"TOTAL:\s*(\d+)\s+PASS:\s*(\d+)\s+WRONG:\s*(\d+)\s+CFAIL:\s*(\d+)\s+RFAIL:\s*(\d+)")
            .unwrap();

    if let Some(cap) = summary_re.captures(output) {
        summary.total = cap[1].parse().unwrap_or(0);
        summary.pass = cap[2].parse().unwrap_or(0);
        summary.wrong = cap[3].parse().unwrap_or(0);
        summary.cfail = cap[4].parse().unwrap_or(0);
        summary.rfail = cap[5].parse().unwrap_or(0);
    }

    // Parse timing line: "Wall: 755ms  Compile: 469ms  Run: 45ms"
    let timing_re =
        Regex::new(r"Wall:\s*(\d+)ms\s+Compile:\s*(\d+)ms\s+Run:\s*(\d+)ms").unwrap();

    if let Some(cap) = timing_re.captures(output) {
        summary.wall_ms = cap[1].parse().unwrap_or(0);
        summary.compile_ms = cap[2].parse().unwrap_or(0);
        summary.run_ms = cap[3].parse().unwrap_or(0);
    }

    // Parse individual results: "name:STATUS:compile_ms:run_ms"
    let result_re = Regex::new(r"(?m)^(\S+):(PASS|WRONG|CFAIL|RFAIL):(\d+):(\d+)").unwrap();

    for cap in result_re.captures_iter(output) {
        let name = cap[1].to_string();
        let status = match &cap[2] {
            "PASS" => TestStatus::Pass,
            "WRONG" => TestStatus::Wrong,
            "CFAIL" => TestStatus::Cfail,
            "RFAIL" => TestStatus::Rfail,
            _ => continue,
        };
        let category = TestCategory::from_test_name(&name);
        summary.results.push(TestResult {
            name,
            status,
            category,
            compile_ms: cap[3].parse().unwrap_or(0),
            run_ms: cap[4].parse().unwrap_or(0),
        });
    }

    Ok(summary)
}

/// Scan the tests directory and return all test files grouped by category.
/// Does NOT run tests - just discovers what exists.
pub fn scan_all_tests(project_root: &Path) -> Vec<TestCategoryGroup> {
    let tests_dir = project_root.join("compiler/tests");
    let mut all_tests: HashMap<TestCategory, Vec<TestResult>> = HashMap::new();

    // Scan main test files
    if let Ok(entries) = fs::read_dir(&tests_dir) {
        for entry in entries.flatten() {
            let path = entry.path();
            if path.is_file() && path.extension().map_or(false, |e| e == "fs") {
                let name = path.file_stem().unwrap().to_string_lossy().to_string();
                // Skip utility files
                if name == "run" || name == "validate" || name.starts_with("debug") {
                    continue;
                }
                let category = TestCategory::from_test_name(&name);
                all_tests.entry(category.clone()).or_default().push(TestResult {
                    name,
                    status: TestStatus::Unknown,
                    category,
                    compile_ms: 0,
                    run_ms: 0,
                });
            }
        }
    }

    // Scan adversarial tests
    let adversarial_dir = tests_dir.join("adversarial");
    if let Ok(entries) = fs::read_dir(&adversarial_dir) {
        for entry in entries.flatten() {
            let path = entry.path();
            if path.is_file() && path.extension().map_or(false, |e| e == "fs") {
                let filename = path.file_stem().unwrap().to_string_lossy().to_string();
                // Skip non-test files
                if filename == "run-all" || !filename.chars().next().map_or(false, |c| c.is_ascii_digit()) {
                    continue;
                }
                let name = format!("adversarial/{}", filename);
                all_tests.entry(TestCategory::Adversarial).or_default().push(TestResult {
                    name,
                    status: TestStatus::Unknown,
                    category: TestCategory::Adversarial,
                    compile_ms: 0,
                    run_ms: 0,
                });
            }
        }
    }

    // Convert to sorted groups
    let mut groups: Vec<TestCategoryGroup> = all_tests
        .into_iter()
        .map(|(category, mut tests)| {
            tests.sort_by(|a, b| a.name.cmp(&b.name));
            TestCategoryGroup {
                display_name: category.display_name().to_string(),
                total: tests.len() as u32,
                pass: 0,
                wrong: 0,
                cfail: 0,
                rfail: 0,
                tests,
                category,
            }
        })
        .collect();

    groups.sort_by_key(|g| g.category.order());
    groups
}

/// Run all tests and return results grouped by category.
pub fn run_all_tests_grouped(project_root: &Path) -> Result<Vec<TestCategoryGroup>, String> {
    let test_script = project_root.join("compiler/tests/test");
    if !test_script.exists() {
        return Err(format!("Test script not found: {}", test_script.display()));
    }

    // Run tests with VERBOSE to get per-test results
    let output = Command::new(&test_script)
        .current_dir(project_root)
        .env("VERBOSE", "1")
        .output()
        .map_err(|e| format!("Failed to run tests: {}", e))?;

    let stdout = String::from_utf8_lossy(&output.stdout).to_string();
    let stderr = String::from_utf8_lossy(&output.stderr).to_string();
    let combined = format!("{}\n{}", stdout, stderr);

    // Parse results
    let summary = parse_test_output(&combined)?;

    // Group by category
    let mut groups: HashMap<TestCategory, TestCategoryGroup> = HashMap::new();

    for test in summary.results {
        let category = test.category.clone();
        let group = groups.entry(category.clone()).or_insert_with(|| TestCategoryGroup {
            category: category.clone(),
            display_name: category.display_name().to_string(),
            total: 0,
            pass: 0,
            wrong: 0,
            cfail: 0,
            rfail: 0,
            tests: Vec::new(),
        });

        group.total += 1;
        match test.status {
            TestStatus::Pass => group.pass += 1,
            TestStatus::Wrong => group.wrong += 1,
            TestStatus::Cfail => group.cfail += 1,
            TestStatus::Rfail => group.rfail += 1,
            TestStatus::Unknown => {}
        }
        group.tests.push(test);
    }

    // Sort groups and tests within groups
    let mut result: Vec<TestCategoryGroup> = groups.into_values().collect();
    result.sort_by_key(|g| g.category.order());
    for group in &mut result {
        group.tests.sort_by(|a, b| a.name.cmp(&b.name));
    }

    Ok(result)
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn test_parse_summary() {
        let output = "............................................\n\n\
                       TOTAL: 44  PASS: 44  WRONG: 0  CFAIL: 0  RFAIL: 0\n\
                       Wall: 755ms  Compile: 469ms  Run: 45ms\n";
        let summary = parse_test_output(output).unwrap();
        assert_eq!(summary.total, 44);
        assert_eq!(summary.pass, 44);
        assert_eq!(summary.wall_ms, 755);
        assert_eq!(summary.compile_ms, 469);
        assert_eq!(summary.run_ms, 45);
    }
}

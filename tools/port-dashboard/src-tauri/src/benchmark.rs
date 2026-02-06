//! Rigorous benchmark runner for comparing Sixth compiler vs GCC at all optimization levels
//!
//! Methodology:
//! - 10 runs per benchmark (configurable)
//! - Reports median, mean, std dev, min, max, P95
//! - Tests GCC -O0, -O1, -O2, -O3
//! - Verifies output correctness
//! - Uses existing /bench/ suite (17 benchmarks)

use std::fs;
use std::path::Path;
use std::process::Command;
use std::time::Instant;

use crate::models::{
    BenchmarkCategory, BenchmarkDef, BenchmarkResult, BenchmarkStatus,
    BenchmarkSummary, GccResults, TimingStats,
};

/// Number of runs per benchmark for statistical significance
const RUNS_PER_BENCHMARK: u32 = 10;

/// GCC binaries path tuple type alias
pub type GccBinaries = (std::path::PathBuf, std::path::PathBuf, std::path::PathBuf, std::path::PathBuf);

/// Compile GCC benchmarks at all optimization levels (public for single benchmark runs)
pub fn prepare_gcc_binaries(project_root: &Path) -> Result<GccBinaries, String> {
    let temp_dir = std::env::temp_dir().join("sixth-benchmarks");
    let _ = std::fs::create_dir_all(&temp_dir);
    compile_gcc_benchmarks(project_root, &temp_dir)
}

/// Benchmark definitions from the existing /bench/ suite
pub fn get_benchmark_definitions() -> Vec<BenchmarkDef> {
    vec![
        BenchmarkDef {
            name: "arith".to_string(),
            description: "nos+ 1-nzloop (custom words)".to_string(),
            forth_source: "bench/arith.fs".to_string(),
            c_test_name: "arith".to_string(),
            category: BenchmarkCategory::Arithmetic,
        },
        BenchmarkDef {
            name: "arith-std".to_string(),
            description: "swap 1+ swap / begin..while (standard)".to_string(),
            forth_source: "bench/arith-std.fs".to_string(),
            c_test_name: "arith-std".to_string(),
            category: BenchmarkCategory::Arithmetic,
        },
        BenchmarkDef {
            name: "loop".to_string(),
            description: "1-nzloop (custom word)".to_string(),
            forth_source: "bench/loop.fs".to_string(),
            c_test_name: "loop".to_string(),
            category: BenchmarkCategory::Loop,
        },
        BenchmarkDef {
            name: "loop-std".to_string(),
            description: "1- dup 0> while repeat (standard)".to_string(),
            forth_source: "bench/loop-std.fs".to_string(),
            c_test_name: "loop-std".to_string(),
            category: BenchmarkCategory::Loop,
        },
        BenchmarkDef {
            name: "fib".to_string(),
            description: "tuck+ in do/loop (custom word)".to_string(),
            forth_source: "bench/fib.fs".to_string(),
            c_test_name: "fib".to_string(),
            category: BenchmarkCategory::Recursion,
        },
        BenchmarkDef {
            name: "fib-std".to_string(),
            description: "swap over + in do/loop (standard)".to_string(),
            forth_source: "bench/fib-std.fs".to_string(),
            c_test_name: "fib-std".to_string(),
            category: BenchmarkCategory::Recursion,
        },
        BenchmarkDef {
            name: "branch".to_string(),
            description: "100M if/else with 1 and".to_string(),
            forth_source: "bench/branch.fs".to_string(),
            c_test_name: "branch".to_string(),
            category: BenchmarkCategory::Branch,
        },
        BenchmarkDef {
            name: "stack".to_string(),
            description: "100M swaps".to_string(),
            forth_source: "bench/stack.fs".to_string(),
            c_test_name: "stack".to_string(),
            category: BenchmarkCategory::Stack,
        },
        BenchmarkDef {
            name: "nested".to_string(),
            description: "10K x 10K do/loop (100M)".to_string(),
            forth_source: "bench/nested.fs".to_string(),
            c_test_name: "nested".to_string(),
            category: BenchmarkCategory::Loop,
        },
        BenchmarkDef {
            name: "mem".to_string(),
            description: "100K x 1K memory write/read".to_string(),
            forth_source: "bench/mem.fs".to_string(),
            c_test_name: "mem".to_string(),
            category: BenchmarkCategory::Memory,
        },
        BenchmarkDef {
            name: "call".to_string(),
            description: "10M recursive countdown".to_string(),
            forth_source: "bench/call.fs".to_string(),
            c_test_name: "call".to_string(),
            category: BenchmarkCategory::Mixed,
        },
        BenchmarkDef {
            name: "collatz".to_string(),
            description: "50K Collatz sequences".to_string(),
            forth_source: "bench/collatz.fs".to_string(),
            c_test_name: "collatz".to_string(),
            category: BenchmarkCategory::Mixed,
        },
        BenchmarkDef {
            name: "spill".to_string(),
            description: "4-var rotate via memory".to_string(),
            forth_source: "bench/spill.fs".to_string(),
            c_test_name: "spill".to_string(),
            category: BenchmarkCategory::Memory,
        },
        BenchmarkDef {
            name: "arith50m".to_string(),
            description: "50M mixed ALU pipeline".to_string(),
            forth_source: "bench/arith50m.fs".to_string(),
            c_test_name: "arith50m".to_string(),
            category: BenchmarkCategory::Arithmetic,
        },
        BenchmarkDef {
            name: "call100m".to_string(),
            description: "100M function calls".to_string(),
            forth_source: "bench/call100m.fs".to_string(),
            c_test_name: "call100m".to_string(),
            category: BenchmarkCategory::Mixed,
        },
        BenchmarkDef {
            name: "fib38".to_string(),
            description: "recursive fib(38)".to_string(),
            forth_source: "bench/fib38.fs".to_string(),
            c_test_name: "fib38".to_string(),
            category: BenchmarkCategory::Recursion,
        },
        BenchmarkDef {
            name: "nested100k".to_string(),
            description: "100K x 10K do/loop".to_string(),
            forth_source: "bench/nested100k.fs".to_string(),
            c_test_name: "nested100k".to_string(),
            category: BenchmarkCategory::Loop,
        },
    ]
}

/// Calculate timing statistics from a vector of microsecond measurements
fn calculate_stats(mut samples: Vec<u64>) -> TimingStats {
    if samples.is_empty() {
        return TimingStats::default();
    }

    samples.sort_unstable();
    let n = samples.len();

    // Median
    let median_us = if n % 2 == 0 {
        (samples[n / 2 - 1] + samples[n / 2]) / 2
    } else {
        samples[n / 2]
    };

    // Mean
    let sum: u64 = samples.iter().sum();
    let mean_us = sum / n as u64;

    // Std dev (sample)
    let variance: f64 = samples
        .iter()
        .map(|&x| {
            let diff = x as f64 - mean_us as f64;
            diff * diff
        })
        .sum::<f64>()
        / (n - 1).max(1) as f64;
    let std_dev_us = variance.sqrt() as u64;

    // Min/Max
    let min_us = *samples.first().unwrap();
    let max_us = *samples.last().unwrap();

    // P95 (95th percentile)
    let p95_idx = ((n as f64) * 0.95).ceil() as usize - 1;
    let p95_us = samples[p95_idx.min(n - 1)];

    TimingStats {
        median_us,
        mean_us,
        std_dev_us,
        min_us,
        max_us,
        p95_us,
        runs: n as u32,
        samples,
    }
}

/// Compile Forth source with Sixth compiler, returning time in microseconds
fn compile_sixth(project_root: &Path, source: &Path, output: &Path) -> Result<u64, String> {
    let engine = project_root.join("fifth");
    let compiler = project_root.join("compiler/sixth.fs");

    if !engine.exists() {
        // Try engine/fifth as fallback
        let alt_engine = project_root.join("engine/fifth");
        if !alt_engine.exists() {
            return Err("fifth binary not found".to_string());
        }
        return compile_sixth_with_engine(project_root, &alt_engine, &compiler, source, output);
    }

    compile_sixth_with_engine(project_root, &engine, &compiler, source, output)
}

fn compile_sixth_with_engine(
    project_root: &Path,
    engine: &Path,
    compiler: &Path,
    source: &Path,
    output: &Path,
) -> Result<u64, String> {
    if !compiler.exists() {
        return Err("compiler/sixth.fs not found".to_string());
    }
    if !source.exists() {
        return Err(format!("Source file not found: {:?}", source));
    }

    let start = Instant::now();
    let result = Command::new(engine)
        .arg(compiler)
        .arg(source)
        .arg(output)
        .current_dir(project_root)
        .output();

    let elapsed = start.elapsed();

    match result {
        Ok(output_result) => {
            if output_result.status.success() {
                Ok(elapsed.as_micros() as u64)
            } else {
                let stderr = String::from_utf8_lossy(&output_result.stderr);
                Err(format!("Exit code {}: {}", output_result.status, stderr))
            }
        }
        Err(e) => Err(format!("Failed to execute: {}", e)),
    }
}

/// Pre-compile bench.c at all optimization levels
fn compile_gcc_benchmarks(
    project_root: &Path,
    temp_dir: &Path,
) -> Result<(std::path::PathBuf, std::path::PathBuf, std::path::PathBuf, std::path::PathBuf), String>
{
    let bench_c = project_root.join("bench/bench.c");
    if !bench_c.exists() {
        return Err("bench/bench.c not found".to_string());
    }

    let o0_out = temp_dir.join("bench_O0");
    let o1_out = temp_dir.join("bench_O1");
    let o2_out = temp_dir.join("bench_O2");
    let o3_out = temp_dir.join("bench_O3");

    // Compile at each optimization level
    for (level, output) in [("-O0", &o0_out), ("-O1", &o1_out), ("-O2", &o2_out), ("-O3", &o3_out)] {
        let result = Command::new("gcc")
            .arg(level)
            .arg("-o")
            .arg(output)
            .arg(&bench_c)
            .output();

        match result {
            Ok(out) => {
                if !out.status.success() {
                    let stderr = String::from_utf8_lossy(&out.stderr);
                    return Err(format!("gcc {} failed: {}", level, stderr));
                }
            }
            Err(e) => return Err(format!("Failed to run gcc: {}", e)),
        }
    }

    Ok((o0_out, o1_out, o2_out, o3_out))
}

/// Run a binary and capture output and timing
fn run_binary_timed(binary: &Path, args: &[&str]) -> Result<(u64, String), String> {
    if !binary.exists() {
        return Err(format!("Binary not found: {:?}", binary));
    }

    // Make executable on Unix
    #[cfg(unix)]
    {
        use std::os::unix::fs::PermissionsExt;
        if let Ok(mut perms) = fs::metadata(binary).and_then(|m| Ok(m.permissions())) {
            perms.set_mode(0o755);
            let _ = fs::set_permissions(binary, perms);
        }
    }

    let start = Instant::now();
    let result = Command::new(binary).args(args).output();
    let elapsed = start.elapsed();

    match result {
        Ok(output) => {
            if output.status.success() {
                let stdout = String::from_utf8_lossy(&output.stdout);
                let first_line = stdout.lines().next().unwrap_or("").trim().to_string();
                Ok((elapsed.as_micros() as u64, first_line))
            } else {
                Err(format!("Exit code {}", output.status))
            }
        }
        Err(e) => Err(format!("Failed to execute: {}", e)),
    }
}

/// Run a binary multiple times and collect timing stats
fn run_binary_stats(binary: &Path, args: &[&str], runs: u32) -> Result<(TimingStats, String), String> {
    let mut samples = Vec::with_capacity(runs as usize);
    let mut output = String::new();

    for i in 0..runs {
        match run_binary_timed(binary, args) {
            Ok((us, out)) => {
                samples.push(us);
                if i == 0 {
                    output = out;
                }
            }
            Err(e) => return Err(e),
        }
    }

    Ok((calculate_stats(samples), output))
}

/// Run a single benchmark with full statistics
pub fn run_benchmark(
    project_root: &Path,
    def: &BenchmarkDef,
    temp_dir: &Path,
    gcc_binaries: &(std::path::PathBuf, std::path::PathBuf, std::path::PathBuf, std::path::PathBuf),
    runs: u32,
) -> BenchmarkResult {
    let forth_path = project_root.join(&def.forth_source);
    let sixth_out = temp_dir.join(format!("sixth_{}", def.name));

    let mut result = BenchmarkResult {
        name: def.name.clone(),
        description: def.description.clone(),
        category: def.category.clone(),
        ..Default::default()
    };

    // Compile with Sixth (single compile, measure time)
    let mut compile_samples = Vec::new();
    for _ in 0..runs.min(3) {
        // Compile 3 times for compile timing
        match compile_sixth(project_root, &forth_path, &sixth_out) {
            Ok(us) => compile_samples.push(us),
            Err(e) => {
                result.status = BenchmarkStatus::SixthFail;
                result.error = Some(format!("Sixth compile failed: {}", e));
                return result;
            }
        }
    }
    result.sixth_compile = calculate_stats(compile_samples);

    // Get Sixth binary size
    if let Ok(metadata) = fs::metadata(&sixth_out) {
        result.sixth_size_bytes = metadata.len();
    }

    // Run Sixth binary multiple times
    match run_binary_stats(&sixth_out, &[], runs) {
        Ok((stats, output)) => {
            result.sixth_run = stats;
            result.sixth_output = output;
        }
        Err(e) => {
            result.status = BenchmarkStatus::SixthFail;
            result.error = Some(format!("Sixth run failed: {}", e));
            return result;
        }
    }

    // Get GCC binary sizes
    if let Ok(m) = fs::metadata(&gcc_binaries.0) {
        result.gcc_size_o0 = m.len();
    }
    if let Ok(m) = fs::metadata(&gcc_binaries.1) {
        result.gcc_size_o1 = m.len();
    }
    if let Ok(m) = fs::metadata(&gcc_binaries.2) {
        result.gcc_size_o2 = m.len();
    }
    if let Ok(m) = fs::metadata(&gcc_binaries.3) {
        result.gcc_size_o3 = m.len();
    }

    // Run GCC at all optimization levels
    let c_test_name = &def.c_test_name;

    // GCC -O0
    match run_binary_stats(&gcc_binaries.0, &[c_test_name], runs) {
        Ok((stats, output)) => {
            result.gcc_run.o0 = stats;
            if result.gcc_output.is_empty() {
                result.gcc_output = output;
            }
        }
        Err(e) => {
            result.status = BenchmarkStatus::GccFail;
            result.error = Some(format!("GCC -O0 run failed: {}", e));
            return result;
        }
    }

    // GCC -O1
    match run_binary_stats(&gcc_binaries.1, &[c_test_name], runs) {
        Ok((stats, _)) => result.gcc_run.o1 = stats,
        Err(e) => {
            result.status = BenchmarkStatus::GccFail;
            result.error = Some(format!("GCC -O1 run failed: {}", e));
            return result;
        }
    }

    // GCC -O2
    match run_binary_stats(&gcc_binaries.2, &[c_test_name], runs) {
        Ok((stats, _)) => result.gcc_run.o2 = stats,
        Err(e) => {
            result.status = BenchmarkStatus::GccFail;
            result.error = Some(format!("GCC -O2 run failed: {}", e));
            return result;
        }
    }

    // GCC -O3
    match run_binary_stats(&gcc_binaries.3, &[c_test_name], runs) {
        Ok((stats, _)) => result.gcc_run.o3 = stats,
        Err(e) => {
            result.status = BenchmarkStatus::GccFail;
            result.error = Some(format!("GCC -O3 run failed: {}", e));
            return result;
        }
    }

    // Calculate ratios (using median times)
    let sixth_median = result.sixth_run.median_us as f64;
    let o2_median = result.gcc_run.o2.median_us as f64;
    let o3_median = result.gcc_run.o3.median_us as f64;

    if o2_median > 0.0 {
        result.ratio_vs_o2 = sixth_median / o2_median;
    }
    if o3_median > 0.0 {
        result.ratio_vs_o3 = sixth_median / o3_median;
    }

    // Check output correctness
    result.output_correct = result.sixth_output == result.gcc_output;

    // Set final status
    if result.output_correct {
        result.status = BenchmarkStatus::Pass;
    } else {
        result.status = BenchmarkStatus::OutputMismatch;
    }

    result
}

/// Run all benchmarks and return summary with full statistics
pub fn run_all_benchmarks(project_root: &Path) -> BenchmarkSummary {
    let temp_dir = std::env::temp_dir().join("sixth-benchmarks");
    let _ = fs::create_dir_all(&temp_dir);

    println!("[benchmark] Compiling GCC benchmarks at all optimization levels...");

    // Pre-compile GCC benchmarks
    let gcc_binaries = match compile_gcc_benchmarks(project_root, &temp_dir) {
        Ok(bins) => bins,
        Err(e) => {
            eprintln!("[benchmark] Failed to compile GCC benchmarks: {}", e);
            return BenchmarkSummary {
                run_at: chrono::Utc::now().to_rfc3339(),
                ..Default::default()
            };
        }
    };

    println!("[benchmark] Running benchmarks ({} runs each)...", RUNS_PER_BENCHMARK);

    let definitions = get_benchmark_definitions();
    let mut results = Vec::new();
    let mut pass = 0u32;
    let mut output_mismatch = 0u32;
    let mut sixth_fail = 0u32;
    let mut gcc_fail = 0u32;
    let mut ratio_products_o2 = 1.0f64;
    let mut ratio_products_o3 = 1.0f64;
    let mut ratio_count = 0u32;

    for def in &definitions {
        println!("[benchmark] Running: {} - {}", def.name, def.description);
        let result = run_benchmark(project_root, def, &temp_dir, &gcc_binaries, RUNS_PER_BENCHMARK);

        match result.status {
            BenchmarkStatus::Pass => {
                pass += 1;
                if result.ratio_vs_o2 > 0.0 {
                    ratio_products_o2 *= result.ratio_vs_o2;
                    ratio_products_o3 *= result.ratio_vs_o3;
                    ratio_count += 1;
                }
                println!(
                    "  PASS: sixth={}us gcc-O2={}us ratio={:.2}x correct={}",
                    result.sixth_run.median_us,
                    result.gcc_run.o2.median_us,
                    result.ratio_vs_o2,
                    result.output_correct
                );
            }
            BenchmarkStatus::OutputMismatch => {
                output_mismatch += 1;
                println!(
                    "  OUTPUT MISMATCH: sixth='{}' gcc='{}'",
                    result.sixth_output, result.gcc_output
                );
            }
            BenchmarkStatus::SixthFail => {
                sixth_fail += 1;
                println!("  SIXTH FAIL: {:?}", result.error);
            }
            BenchmarkStatus::GccFail => {
                gcc_fail += 1;
                println!("  GCC FAIL: {:?}", result.error);
            }
            BenchmarkStatus::BothFail => {
                sixth_fail += 1;
                gcc_fail += 1;
                println!("  BOTH FAIL: {:?}", result.error);
            }
            BenchmarkStatus::Skipped => {
                println!("  SKIPPED");
            }
        }

        results.push(result);
    }

    // Calculate geometric means
    let geomean_ratio_o2 = if ratio_count > 0 {
        ratio_products_o2.powf(1.0 / ratio_count as f64)
    } else {
        0.0
    };
    let geomean_ratio_o3 = if ratio_count > 0 {
        ratio_products_o3.powf(1.0 / ratio_count as f64)
    } else {
        0.0
    };

    println!("[benchmark] Done. {} pass, {} mismatch, {} sixth fail, {} gcc fail",
        pass, output_mismatch, sixth_fail, gcc_fail);
    println!("[benchmark] Geometric mean ratio vs GCC -O2: {:.2}x", geomean_ratio_o2);

    BenchmarkSummary {
        run_at: chrono::Utc::now().to_rfc3339(),
        total: definitions.len() as u32,
        pass,
        output_mismatch,
        sixth_fail,
        gcc_fail,
        runs_per_benchmark: RUNS_PER_BENCHMARK,
        geomean_ratio_o2,
        geomean_ratio_o3,
        results,
    }
}

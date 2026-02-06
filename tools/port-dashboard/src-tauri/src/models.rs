use serde::{Deserialize, Serialize};

#[derive(Debug, Clone, Serialize, Deserialize, PartialEq)]
#[serde(rename_all = "lowercase")]
pub enum WordStatus {
    Done,
    Pending,
    Redesign,
    Eliminated,
}

impl WordStatus {
    pub fn as_str(&self) -> &'static str {
        match self {
            WordStatus::Done => "done",
            WordStatus::Pending => "pending",
            WordStatus::Redesign => "redesign",
            WordStatus::Eliminated => "eliminated",
        }
    }

    pub fn from_str(s: &str) -> Self {
        match s {
            "done" => WordStatus::Done,
            "redesign" => WordStatus::Redesign,
            "eliminated" => WordStatus::Eliminated,
            _ => WordStatus::Pending,
        }
    }
}

#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct Word {
    pub word: String,
    pub phase: u8,
    pub file: String,
    pub status: WordStatus,
    pub note: String,
    /// Adversarial tests that cover this word
    #[serde(default)]
    pub tests: Vec<String>,
    /// Number of covering tests that pass
    #[serde(default)]
    pub tests_passing: u32,
    /// Number of covering tests that fail
    #[serde(default)]
    pub tests_failing: u32,
}

#[derive(Debug, Clone, Serialize, Deserialize, PartialEq)]
#[serde(rename_all = "lowercase")]
pub enum PhaseStatus {
    Done,
    Active,
    Stub,
    Pending,
}

#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct Gate {
    pub text: String,
    pub satisfied: bool,
    /// true if this gate is auto-computed from file state
    pub auto_computed: bool,
}

#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct Phase {
    pub id: u8,
    pub name: String,
    pub desc: String,
    pub file: String,
    pub status: PhaseStatus,
    pub gates_in: Vec<Gate>,
    pub gates_out: Vec<Gate>,
    pub deliverables: Vec<String>,
    pub risk: String,
    pub words: WordCounts,
}

#[derive(Debug, Clone, Serialize, Deserialize, Default)]
pub struct WordCounts {
    pub done: u32,
    pub pending: u32,
    pub redesign: u32,
    pub eliminated: u32,
}

/// Live file inventory entry — scanned from actual filesystem
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct FileInfo {
    pub name: String,
    pub category: String, // "arch-arm64", "arch-x86", "shared", "unchanged"
    pub phase: u8,
    pub lines: u32,
    pub word_defs: u32,
    pub constants: u32,
    pub x86_ref: String,
    pub x86_lines: u32,
    pub x86_words: u32,
    pub words: Vec<String>, // actual word names defined
    pub status: String,     // "done", "stub", "active", "pending", "patch", "unchanged"
}

#[derive(Debug, Clone, Serialize, Deserialize, PartialEq)]
#[serde(rename_all = "UPPERCASE")]
pub enum TestStatus {
    Pass,
    Wrong,
    Cfail,
    Rfail,
    Unknown,
}

/// Test categories based on naming conventions
#[derive(Debug, Clone, Serialize, Deserialize, PartialEq, Eq, Hash)]
#[serde(rename_all = "lowercase")]
pub enum TestCategory {
    Basic,        // 01-99: primitives
    Folding,      // 1000-1015: constant folding
    Fusing,       // 1016-1030: instruction fusing
    Forward,      // 1031-1049: forward references
    Control,      // 1100-1199, control flow patterns
    Memory,       // 1200-1299, memory operations
    Strings,      // 1300-1399, string operations
    Double,       // 1400-1499, double-cell operations
    Algorithms,   // 1500-1599, algorithm tests
    Variables,    // 1600-1699, variable/loop interaction
    Strength,     // 2100-2199, strength reduction
    Parsing,      // 2200-2299, parsing primitives
    Stress,       // 3000+, stress tests
    Adversarial,  // adversarial/ directory
    Hayes,        // hayes/ directory (ANS compliance)
    Other,        // everything else
}

impl TestCategory {
    pub fn from_test_name(name: &str) -> Self {
        // Check for directory prefixes
        if name.starts_with("adversarial/") || name.contains("adversarial") {
            return TestCategory::Adversarial;
        }
        if name.starts_with("hayes/") {
            return TestCategory::Hayes;
        }

        // Extract numeric prefix
        let prefix: String = name.chars().take_while(|c| c.is_ascii_digit()).collect();
        if let Ok(num) = prefix.parse::<u32>() {
            match num {
                0..=99 => TestCategory::Basic,
                1000..=1015 => TestCategory::Folding,
                1016..=1030 => TestCategory::Fusing,
                1031..=1049 => TestCategory::Forward,
                1050..=1099 => TestCategory::Other,
                1100..=1199 => TestCategory::Control,
                1200..=1299 => TestCategory::Memory,
                1300..=1399 => TestCategory::Strings,
                1400..=1499 => TestCategory::Double,
                1500..=1599 => TestCategory::Algorithms,
                1600..=1699 => TestCategory::Variables,
                2100..=2199 => TestCategory::Strength,
                2200..=2299 => TestCategory::Parsing,
                3000..=3999 => TestCategory::Stress,
                _ => TestCategory::Other,
            }
        } else {
            TestCategory::Other
        }
    }

    pub fn display_name(&self) -> &'static str {
        match self {
            TestCategory::Basic => "Basic Primitives",
            TestCategory::Folding => "Constant Folding",
            TestCategory::Fusing => "Instruction Fusing",
            TestCategory::Forward => "Forward References",
            TestCategory::Control => "Control Flow",
            TestCategory::Memory => "Memory Operations",
            TestCategory::Strings => "String Operations",
            TestCategory::Double => "Double-Cell",
            TestCategory::Algorithms => "Algorithms",
            TestCategory::Variables => "Variables & Loops",
            TestCategory::Strength => "Strength Reduction",
            TestCategory::Parsing => "Parsing",
            TestCategory::Stress => "Stress Tests",
            TestCategory::Adversarial => "Adversarial",
            TestCategory::Hayes => "ANS Compliance",
            TestCategory::Other => "Other",
        }
    }

    pub fn order(&self) -> u8 {
        match self {
            TestCategory::Basic => 0,
            TestCategory::Folding => 1,
            TestCategory::Fusing => 2,
            TestCategory::Forward => 3,
            TestCategory::Control => 4,
            TestCategory::Memory => 5,
            TestCategory::Strings => 6,
            TestCategory::Double => 7,
            TestCategory::Variables => 8,
            TestCategory::Algorithms => 9,
            TestCategory::Strength => 10,
            TestCategory::Parsing => 11,
            TestCategory::Stress => 12,
            TestCategory::Adversarial => 13,
            TestCategory::Hayes => 14,
            TestCategory::Other => 15,
        }
    }
}

#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct TestResult {
    pub name: String,
    pub status: TestStatus,
    pub category: TestCategory,
    pub compile_ms: u32,
    pub run_ms: u32,
}

/// Grouped test results by category
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct TestCategoryGroup {
    pub category: TestCategory,
    pub display_name: String,
    pub total: u32,
    pub pass: u32,
    pub wrong: u32,
    pub cfail: u32,
    pub rfail: u32,
    pub tests: Vec<TestResult>,
}

#[derive(Debug, Clone, Serialize, Deserialize, Default)]
pub struct TestSummary {
    pub total: u32,
    pub pass: u32,
    pub wrong: u32,
    pub cfail: u32,
    pub rfail: u32,
    pub wall_ms: u32,
    pub compile_ms: u32,
    pub run_ms: u32,
    pub results: Vec<TestResult>,
}

#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct TestRun {
    pub id: i64,
    pub run_at: String,
    pub total: u32,
    pub pass: u32,
    pub wrong: u32,
    pub cfail: u32,
    pub rfail: u32,
    pub wall_ms: u32,
    pub compile_ms: u32,
    pub run_ms: u32,
}

// ============================================================
// BENCHMARK MODELS
// ============================================================

/// A benchmark definition - comparing Sixth compiler vs GCC at multiple optimization levels
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct BenchmarkDef {
    pub name: String,
    pub description: String,
    /// Forth source file path (relative to project root)
    pub forth_source: String,
    /// C function name in bench.c
    pub c_test_name: String,
    /// Category for grouping
    pub category: BenchmarkCategory,
}

#[derive(Debug, Clone, Serialize, Deserialize, PartialEq, Eq, Hash)]
#[serde(rename_all = "lowercase")]
pub enum BenchmarkCategory {
    Arithmetic,   // ALU operations, arithmetic patterns
    Loop,         // Loop constructs
    Recursion,    // Recursive function calls
    Memory,       // Memory access patterns
    Branch,       // Branching patterns
    Stack,        // Stack manipulation
    Mixed,        // Mixed operations
}

impl BenchmarkCategory {
    pub fn from_name(name: &str) -> Self {
        match name {
            "arith" | "arith-std" | "arith50m" => BenchmarkCategory::Arithmetic,
            "loop" | "loop-std" | "nested" | "nested100k" => BenchmarkCategory::Loop,
            "fib" | "fib-std" | "fib38" | "fibrec" => BenchmarkCategory::Recursion,
            "mem" | "spill" => BenchmarkCategory::Memory,
            "branch" => BenchmarkCategory::Branch,
            "stack" => BenchmarkCategory::Stack,
            "call" | "call100m" | "collatz" => BenchmarkCategory::Mixed,
            _ => BenchmarkCategory::Mixed,
        }
    }
}

/// Statistical summary of multiple timing measurements
#[derive(Debug, Clone, Serialize, Deserialize, Default)]
pub struct TimingStats {
    /// Median time in microseconds
    pub median_us: u64,
    /// Mean time in microseconds
    pub mean_us: u64,
    /// Standard deviation in microseconds
    pub std_dev_us: u64,
    /// Minimum time in microseconds
    pub min_us: u64,
    /// Maximum time in microseconds
    pub max_us: u64,
    /// 95th percentile in microseconds
    pub p95_us: u64,
    /// Number of runs
    pub runs: u32,
    /// All individual measurements (for histogram display)
    pub samples: Vec<u64>,
}

/// GCC results at different optimization levels
#[derive(Debug, Clone, Serialize, Deserialize, Default)]
pub struct GccResults {
    pub o0: TimingStats,
    pub o1: TimingStats,
    pub o2: TimingStats,
    pub o3: TimingStats,
}

/// Result of running a single benchmark with full statistics
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct BenchmarkResult {
    pub name: String,
    pub description: String,
    pub category: BenchmarkCategory,

    /// Sixth compilation time stats
    pub sixth_compile: TimingStats,
    /// Sixth runtime stats
    pub sixth_run: TimingStats,
    /// Sixth binary size in bytes
    pub sixth_size_bytes: u64,

    /// GCC results at all optimization levels
    pub gcc_compile: GccResults,
    /// GCC runtime at all optimization levels
    pub gcc_run: GccResults,
    /// GCC binary sizes at each level
    pub gcc_size_o0: u64,
    pub gcc_size_o1: u64,
    pub gcc_size_o2: u64,
    pub gcc_size_o3: u64,

    /// Sixth output (first line, trimmed)
    pub sixth_output: String,
    /// GCC output (first line, trimmed)
    pub gcc_output: String,
    /// Whether outputs match
    pub output_correct: bool,

    /// Runtime ratio vs GCC -O2: Sixth / GCC-O2 (lower = faster Sixth)
    pub ratio_vs_o2: f64,
    /// Runtime ratio vs GCC -O3
    pub ratio_vs_o3: f64,

    /// Status of benchmark run
    pub status: BenchmarkStatus,
    /// Error message if failed
    pub error: Option<String>,
}

impl Default for BenchmarkResult {
    fn default() -> Self {
        Self {
            name: String::new(),
            description: String::new(),
            category: BenchmarkCategory::Mixed,
            sixth_compile: TimingStats::default(),
            sixth_run: TimingStats::default(),
            sixth_size_bytes: 0,
            gcc_compile: GccResults::default(),
            gcc_run: GccResults::default(),
            gcc_size_o0: 0,
            gcc_size_o1: 0,
            gcc_size_o2: 0,
            gcc_size_o3: 0,
            sixth_output: String::new(),
            gcc_output: String::new(),
            output_correct: false,
            ratio_vs_o2: 0.0,
            ratio_vs_o3: 0.0,
            status: BenchmarkStatus::Skipped,
            error: None,
        }
    }
}

#[derive(Debug, Clone, Serialize, Deserialize, PartialEq)]
#[serde(rename_all = "lowercase")]
pub enum BenchmarkStatus {
    Pass,       // Both compiled and ran successfully, output correct
    OutputMismatch, // Ran but output differs
    SixthFail,  // Sixth compilation or run failed
    GccFail,    // GCC compilation or run failed
    BothFail,   // Both failed
    Skipped,    // Benchmark was skipped
}

/// Summary of a benchmark run
#[derive(Debug, Clone, Serialize, Deserialize, Default)]
pub struct BenchmarkSummary {
    pub run_at: String,
    pub total: u32,
    pub pass: u32,
    pub output_mismatch: u32,
    pub sixth_fail: u32,
    pub gcc_fail: u32,
    /// Number of runs per benchmark
    pub runs_per_benchmark: u32,
    /// Geometric mean of Sixth/GCC-O2 ratios (lower = faster Sixth)
    pub geomean_ratio_o2: f64,
    /// Geometric mean of Sixth/GCC-O3 ratios
    pub geomean_ratio_o3: f64,
    /// Individual results
    pub results: Vec<BenchmarkResult>,
}

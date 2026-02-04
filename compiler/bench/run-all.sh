#!/bin/bash
# run-all.sh - Master benchmark runner for Sixth compiler
# Compiles and runs all benchmarks, comparing Sixth vs GCC performance

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
cd "$SCRIPT_DIR"

# Configuration
COMPILER="${1:-$SCRIPT_DIR/../engine/fifth.fs}"
RUNS="${RUNS:-10}"
TIMEOUT_SEC=120
TIE_THRESHOLD=0.05  # 5% tolerance for tie
WIN_THRESHOLD=0.80  # 80% win rate for success

# Paths
SIXTH_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
FIFTH="$SIXTH_ROOT/fifth"
RESULTS_DIR="$SCRIPT_DIR/results"
TIMESTAMP=$(date +%Y%m%d_%H%M%S)
CSV_FILE="$RESULTS_DIR/bench_${TIMESTAMP}.csv"
TMP_DIR="/tmp/sixth_bench_$$"

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

# Cleanup on exit
cleanup() {
    rm -rf "$TMP_DIR"
}
trap cleanup EXIT

mkdir -p "$TMP_DIR" "$RESULTS_DIR"

# Check prerequisites
if [[ ! -x "$FIFTH" ]]; then
    echo "Error: Fifth interpreter not found at $FIFTH"
    exit 1
fi

if [[ ! -f "$COMPILER" ]]; then
    echo "Error: Sixth compiler not found at $COMPILER"
    exit 1
fi

# Statistics functions
median() {
    # Read numbers from stdin, output median
    sort -n | awk '{a[NR]=$1} END {
        if (NR % 2 == 1) print a[(NR+1)/2]
        else print (a[NR/2] + a[NR/2+1]) / 2
    }'
}

stddev() {
    # Read numbers from stdin, output stddev
    awk '{sum+=$1; sumsq+=$1*$1; n++} END {
        if (n > 1) print sqrt((sumsq - sum*sum/n) / (n-1))
        else print 0
    }'
}

# Time a command, return seconds as float
time_cmd() {
    local cmd="$1"
    local start end elapsed
    start=$(date +%s.%N)
    if timeout ${TIMEOUT_SEC}s "$cmd" >/dev/null 2>&1; then
        end=$(date +%s.%N)
        echo "$end - $start" | bc
    else
        echo "-1"
    fi
}

# Run benchmark N times, return median time
run_benchmark() {
    local binary="$1"
    local times=()
    local t

    for ((i=1; i<=RUNS; i++)); do
        t=$(time_cmd "$binary")
        if [[ "$t" == "-1" ]]; then
            echo "-1"
            return
        fi
        times+=("$t")
    done

    # Calculate median
    printf '%s\n' "${times[@]}" | median
}

# Compile Sixth benchmark
compile_sixth() {
    local name="$1"
    local src="$SCRIPT_DIR/${name}.fs"
    local out="$TMP_DIR/${name}_sixth"

    if timeout 60s "$FIFTH" "$COMPILER" "$src" "$out" 2>/dev/null; then
        if [[ -x "$out" ]]; then
            echo "$out"
            return 0
        fi
    fi
    echo ""
    return 1
}

# Compile GCC benchmark
compile_gcc() {
    local name="$1"
    local src="$SCRIPT_DIR/${name}.c"
    local out="$TMP_DIR/${name}_gcc"

    if gcc -O2 -o "$out" "$src" 2>/dev/null; then
        echo "$out"
        return 0
    fi
    echo ""
    return 1
}

# Verify output matches expected
verify_output() {
    local binary="$1"
    local expected="$2"

    if [[ -z "$expected" ]]; then
        return 0  # No expected value, skip verification
    fi

    local actual
    actual=$(timeout 10s "$binary" 2>/dev/null | head -1 | tr -d '\n\r')

    if [[ "$actual" == "$expected" ]]; then
        return 0
    fi
    return 1
}

# Main benchmark loop
echo "=== Sixth Compiler Benchmark Suite ==="
echo "Compiler: $COMPILER"
echo "Runs per benchmark: $RUNS"
echo "Timeout: ${TIMEOUT_SEC}s"
echo

# Initialize CSV
echo "name,sixth_median,sixth_stddev,gcc_median,gcc_stddev,ratio,result" > "$CSV_FILE"

# Counters
total=0
wins=0
losses=0
ties=0
sixth_fails=0
gcc_fails=0
output_fails=0
skipped=0

# Arrays for ratio calculation
declare -a ratios

# Find all benchmarks with both .fs and .c
for fs_file in *.fs; do
    [[ "$fs_file" == "run.fs" ]] && continue

    name="${fs_file%.fs}"
    c_file="${name}.c"

    # Skip if no C equivalent
    if [[ ! -f "$c_file" ]]; then
        ((skipped++)) || true
        continue
    fi

    ((total++)) || true

    printf "%-20s " "$name"

    # Get expected output
    expected=$(grep -E '^\\\ expected:' "$fs_file" 2>/dev/null | sed 's/\\ expected: *//' || true)

    # Compile Sixth
    sixth_bin=$(compile_sixth "$name")
    if [[ -z "$sixth_bin" ]]; then
        printf "${RED}SIXTH-FAIL${NC}\n"
        echo "$name,-1,-1,-1,-1,-1,sixth-fail" >> "$CSV_FILE"
        ((sixth_fails++)) || true
        continue
    fi

    # Compile GCC
    gcc_bin=$(compile_gcc "$name")
    if [[ -z "$gcc_bin" ]]; then
        printf "${RED}GCC-FAIL${NC}\n"
        echo "$name,-1,-1,-1,-1,-1,gcc-fail" >> "$CSV_FILE"
        ((gcc_fails++)) || true
        continue
    fi

    # Verify Sixth output
    if ! verify_output "$sixth_bin" "$expected"; then
        printf "${RED}OUTPUT-FAIL${NC}\n"
        echo "$name,-1,-1,-1,-1,-1,output-fail" >> "$CSV_FILE"
        ((output_fails++)) || true
        continue
    fi

    # Run benchmarks
    sixth_times=()
    gcc_times=()

    for ((i=1; i<=RUNS; i++)); do
        t=$(time_cmd "$sixth_bin")
        [[ "$t" == "-1" ]] && break
        sixth_times+=("$t")
    done

    for ((i=1; i<=RUNS; i++)); do
        t=$(time_cmd "$gcc_bin")
        [[ "$t" == "-1" ]] && break
        gcc_times+=("$t")
    done

    # Check for timeout
    if [[ ${#sixth_times[@]} -lt $RUNS ]]; then
        printf "${RED}TIMEOUT${NC}\n"
        echo "$name,-1,-1,-1,-1,-1,timeout" >> "$CSV_FILE"
        ((sixth_fails++)) || true
        continue
    fi

    if [[ ${#gcc_times[@]} -lt $RUNS ]]; then
        printf "${RED}GCC-TIMEOUT${NC}\n"
        echo "$name,-1,-1,-1,-1,-1,gcc-timeout" >> "$CSV_FILE"
        ((gcc_fails++)) || true
        continue
    fi

    # Calculate statistics
    sixth_median=$(printf '%s\n' "${sixth_times[@]}" | median)
    sixth_std=$(printf '%s\n' "${sixth_times[@]}" | stddev)
    gcc_median=$(printf '%s\n' "${gcc_times[@]}" | median)
    gcc_std=$(printf '%s\n' "${gcc_times[@]}" | stddev)

    # Calculate ratio (Sixth / GCC)
    if (( $(echo "$gcc_median > 0" | bc -l) )); then
        ratio=$(echo "scale=4; $sixth_median / $gcc_median" | bc)
    else
        ratio="999"
    fi

    ratios+=("$ratio")

    # Determine result
    if (( $(echo "$ratio < 1.0" | bc -l) )); then
        result="WIN"
        result_color="$GREEN"
        ((wins++)) || true
    elif (( $(echo "$ratio <= 1.0 + $TIE_THRESHOLD" | bc -l) )); then
        result="TIE"
        result_color="$BLUE"
        ((ties++)) || true
    else
        result="LOSS"
        result_color="$RED"
        ((losses++)) || true
    fi

    # Output
    printf "${result_color}%s${NC} (%.3fs vs %.3fs, ratio=%.2fx)\n" \
        "$result" "$sixth_median" "$gcc_median" "$ratio"

    echo "$name,$sixth_median,$sixth_std,$gcc_median,$gcc_std,$ratio,$result" >> "$CSV_FILE"
done

echo
echo "=== Summary ==="
echo

# Calculate aggregate statistics
if [[ ${#ratios[@]} -gt 0 ]]; then
    median_ratio=$(printf '%s\n' "${ratios[@]}" | median)
    sorted_ratios=($(printf '%s\n' "${ratios[@]}" | sort -n))
    n=${#sorted_ratios[@]}
    p95_idx=$(echo "($n * 95 + 99) / 100" | bc)
    [[ $p95_idx -ge $n ]] && p95_idx=$((n - 1))
    p95_ratio=${sorted_ratios[$p95_idx]}
else
    median_ratio="N/A"
    p95_ratio="N/A"
fi

# Human-readable table
printf "%-20s %s\n" "Total benchmarks:" "$total"
printf "%-20s ${GREEN}%d${NC}\n" "Wins:" "$wins"
printf "%-20s ${BLUE}%d${NC}\n" "Ties:" "$ties"
printf "%-20s ${RED}%d${NC}\n" "Losses:" "$losses"
echo
printf "%-20s %d\n" "Sixth failures:" "$sixth_fails"
printf "%-20s %d\n" "GCC failures:" "$gcc_fails"
printf "%-20s %d\n" "Output mismatches:" "$output_fails"
printf "%-20s %d\n" "Skipped (no .c):" "$skipped"
echo
printf "%-20s %s\n" "Median ratio:" "$median_ratio"
printf "%-20s %s\n" "95th %ile ratio:" "$p95_ratio"
echo
echo "Results saved to: $CSV_FILE"

# Calculate win rate (wins + ties count as success)
if [[ $total -gt 0 ]]; then
    successful=$((wins + ties))
    win_rate=$(echo "scale=4; $successful / $total" | bc)
    win_pct=$(echo "scale=1; $win_rate * 100" | bc)
    printf "Win+Tie rate: %.1f%%\n" "$win_pct"

    if (( $(echo "$win_rate >= $WIN_THRESHOLD" | bc -l) )); then
        printf "${GREEN}PASSED${NC} (>= %.0f%% required)\n" "$(echo "$WIN_THRESHOLD * 100" | bc)"
        exit 0
    else
        printf "${RED}FAILED${NC} (< %.0f%% threshold)\n" "$(echo "$WIN_THRESHOLD * 100" | bc)"
        exit 1
    fi
else
    echo "No benchmarks ran successfully"
    exit 1
fi

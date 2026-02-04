#!/bin/bash
# validate.sh - Validate benchmark suite integrity
# Checks that all .fs files have matching .c files, compiles all C files,
# and verifies expected: comments match.

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
cd "$SCRIPT_DIR"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo "=== Benchmark Validation ==="
echo

# Counters
total_fs=0
missing_c=0
compile_fail=0
expected_mismatch=0
run_fail=0
valid=0

# Track issues
declare -a missing_c_files
declare -a compile_failures
declare -a expected_mismatches
declare -a run_failures

# Find all .fs benchmark files (exclude run.fs and other non-benchmark files)
for fs_file in *.fs; do
    # Skip non-benchmark files
    [[ "$fs_file" == "run.fs" ]] && continue

    name="${fs_file%.fs}"
    c_file="${name}.c"

    ((total_fs++)) || true

    # Check for matching .c file
    if [[ ! -f "$c_file" ]]; then
        ((missing_c++)) || true
        missing_c_files+=("$name")
        continue
    fi

    # Extract expected values
    fs_expected=$(grep -E '^\\\ expected:' "$fs_file" 2>/dev/null | sed 's/\\ expected: *//' || true)
    c_expected=$(grep -E '^// expected:' "$c_file" 2>/dev/null | sed 's/\/\/ expected: *//' || true)

    # Check expected comments match
    if [[ -n "$fs_expected" && -n "$c_expected" ]]; then
        if [[ "$fs_expected" != "$c_expected" ]]; then
            ((expected_mismatch++)) || true
            expected_mismatches+=("$name: fs='$fs_expected' c='$c_expected'")
        fi
    fi

    # Try to compile C file
    if ! gcc -O2 -o "/tmp/bench_validate_${name}" "$c_file" 2>/dev/null; then
        ((compile_fail++)) || true
        compile_failures+=("$name")
        continue
    fi

    # Try to run C benchmark (with timeout)
    if ! timeout 10s "/tmp/bench_validate_${name}" >/dev/null 2>&1; then
        ((run_fail++)) || true
        run_failures+=("$name")
        rm -f "/tmp/bench_validate_${name}"
        continue
    fi

    rm -f "/tmp/bench_validate_${name}"
    ((valid++)) || true
done

# Report results
echo "=== Summary ==="
echo
printf "Total .fs benchmarks: %d\n" "$total_fs"
printf "${GREEN}Valid benchmarks:     %d${NC}\n" "$valid"

if [[ ${#missing_c_files[@]} -gt 0 ]]; then
    printf "${YELLOW}Missing .c files:     %d${NC}\n" "$missing_c"
    echo "  Files without C equivalent:"
    for f in "${missing_c_files[@]}"; do
        echo "    - $f.fs"
    done
fi

if [[ ${#compile_failures[@]} -gt 0 ]]; then
    printf "${RED}Compile failures:     %d${NC}\n" "$compile_fail"
    echo "  Failed to compile:"
    for f in "${compile_failures[@]}"; do
        echo "    - $f.c"
    done
fi

if [[ ${#expected_mismatches[@]} -gt 0 ]]; then
    printf "${YELLOW}Expected mismatches:  %d${NC}\n" "$expected_mismatch"
    echo "  Mismatched expected values:"
    for m in "${expected_mismatches[@]}"; do
        echo "    - $m"
    done
fi

if [[ ${#run_failures[@]} -gt 0 ]]; then
    printf "${RED}Run failures:         %d${NC}\n" "$run_fail"
    echo "  Failed to run:"
    for f in "${run_failures[@]}"; do
        echo "    - $f"
    done
fi

echo
echo "=== Validation Complete ==="

# Exit code based on critical failures
if [[ "$compile_fail" -gt 0 || "$run_fail" -gt 0 ]]; then
    exit 1
fi

exit 0

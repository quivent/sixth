#!/bin/bash
# Run all 80 fusion pattern tests.
# Compiles each .fs file, runs it, checks correctness, reports timing.

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
PROJECT_DIR="$(cd "$SCRIPT_DIR/../.." && pwd)"
COMPILER="$PROJECT_DIR/fifth"
TF="$PROJECT_DIR/compiler/sixth.fs"
TMPDIR=$(mktemp -d)
trap "rm -rf $TMPDIR" EXIT

# Counters
PASS=0
FAIL=0
SKIP=0
COMPILE_FAIL=0
TOTAL=0
TOTAL_COMPILE_MS=0
TOTAL_RUN_MS=0

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[0;33m'
NC='\033[0m'

printf "%-6s %-28s %-10s %-12s %-12s %s\n" \
    "ID" "Pattern" "Result" "Compile(ms)" "Run(ms)" "Details"
printf "%.0s-" {1..100}
echo

for testfile in "$SCRIPT_DIR"/[A-F]*.fs; do
    [ -f "$testfile" ] || continue
    TOTAL=$((TOTAL + 1))

    basename=$(basename "$testfile" .fs)
    test_id="${basename}"

    # Extract expected output
    expected=$(head -1 "$testfile" | sed 's/\\ expect: //')

    # Extract pattern name
    pattern=$(sed -n '2s/\\ Pattern [A-F][0-9]*: //p' "$testfile")

    outbin="$TMPDIR/$test_id"

    # Handle SKIP tests (unimplemented words)
    if [ "$expected" = "SKIP" ]; then
        SKIP=$((SKIP + 1))
        printf "%-6s %-28s ${YELLOW}%-10s${NC} %-12s %-12s %s\n" \
            "$test_id" "$pattern" "SKIP" "-" "-" "unimplemented word"
        continue
    fi

    # Compile with timing
    compile_start=$(date +%s%N)
    compile_err=""
    if ! $COMPILER $TF "$testfile" "$outbin" 2>"$TMPDIR/compile_err.txt"; then
        compile_err=$(cat "$TMPDIR/compile_err.txt")
    fi
    compile_end=$(date +%s%N)
    compile_ms=$(( (compile_end - compile_start) / 1000000 ))
    TOTAL_COMPILE_MS=$((TOTAL_COMPILE_MS + compile_ms))

    if [ ! -x "$outbin" ] && [ ! -f "$outbin" ]; then
        COMPILE_FAIL=$((COMPILE_FAIL + 1))
        printf "%-6s %-28s ${RED}%-10s${NC} %-12s %-12s %s\n" \
            "$test_id" "$pattern" "CFAIL" "${compile_ms}" "-" "compile failed: $(head -1 "$TMPDIR/compile_err.txt" 2>/dev/null)"
        continue
    fi

    # Make executable if needed
    chmod +x "$outbin" 2>/dev/null || true

    # Run with timing
    run_start=$(date +%s%N)
    actual=$("$outbin" 2>/dev/null) || actual="RUNTIME_ERROR"
    run_end=$(date +%s%N)
    run_ms=$(( (run_end - run_start) / 1000000 ))
    TOTAL_RUN_MS=$((TOTAL_RUN_MS + run_ms))

    # Strip trailing whitespace for comparison
    actual_clean=$(echo "$actual" | sed 's/[[:space:]]*$//')
    expected_clean=$(echo "$expected" | sed 's/[[:space:]]*$//')

    if [ "$actual_clean" = "$expected_clean" ]; then
        PASS=$((PASS + 1))
        printf "%-6s %-28s ${GREEN}%-10s${NC} %-12s %-12s\n" \
            "$test_id" "$pattern" "PASS" "${compile_ms}" "${run_ms}"
    else
        FAIL=$((FAIL + 1))
        printf "%-6s %-28s ${RED}%-10s${NC} %-12s %-12s expected='%s' got='%s'\n" \
            "$test_id" "$pattern" "FAIL" "${compile_ms}" "${run_ms}" \
            "$expected_clean" "$actual_clean"
    fi
done

echo
printf "%.0s=" {1..100}
echo
echo "Results: $PASS pass, $FAIL fail, $SKIP skip, $COMPILE_FAIL compile-fail, $TOTAL total"
echo "Compile time total: ${TOTAL_COMPILE_MS}ms (avg: $((TOTAL_COMPILE_MS / (TOTAL > 0 ? TOTAL : 1)))ms)"
echo "Run time total:     ${TOTAL_RUN_MS}ms (avg: $((TOTAL_RUN_MS / (TOTAL > 0 ? TOTAL : 1)))ms)"

if [ $FAIL -gt 0 ] || [ $COMPILE_FAIL -gt 0 ]; then
    exit 1
fi

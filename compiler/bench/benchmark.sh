#!/bin/bash
# Sixth vs GCC -O2 Benchmark Suite (Parallel)
# Usage: ./compiler/bench/benchmark.sh

set -e
cd "$(dirname "$0")/../.."

COMPILER="compiler/sixth.fs"
BENCHDIR="compiler/bench"
TMPDIR="/tmp/sixth-bench-$$"
RESULTS="$TMPDIR/results"
TIMEOUT=30

mkdir -p "$TMPDIR"
trap "rm -rf $TMPDIR" EXIT

# Run one benchmark
run_bench() {
    local fsfile="$1"
    local bench=$(basename "$fsfile" .fs)
    local cfile="$BENCHDIR/${bench}.c"

    [[ ! -f "$cfile" ]] && return

    local bin_s="$TMPDIR/s_$bench"
    local bin_c="$TMPDIR/c_$bench"

    # Compile Sixth
    local c_s_start=$(date +%s%N)
    if ! ./engine/fifth "$COMPILER" "$fsfile" "$bin_s" >/dev/null 2>&1; then
        echo "$bench:CFAIL:0:0:0:0"
        return
    fi
    local c_s_end=$(date +%s%N)
    local c_s_ms=$(( (c_s_end - c_s_start) / 1000000 ))

    # Compile GCC -O2
    local c_g_start=$(date +%s%N)
    if ! gcc -O2 -o "$bin_c" "$cfile" 2>/dev/null; then
        echo "$bench:GCFAIL:$c_s_ms:0:0:0"
        return
    fi
    local c_g_end=$(date +%s%N)
    local c_g_ms=$(( (c_g_end - c_g_start) / 1000000 ))

    # Run Sixth binary
    local r_s_start=$(date +%s%N)
    if ! timeout $TIMEOUT "$bin_s" </dev/null >/dev/null 2>&1; then
        echo "$bench:RFAIL:$c_s_ms:$c_g_ms:0:0"
        return
    fi
    local r_s_end=$(date +%s%N)
    local r_s_ms=$(( (r_s_end - r_s_start) / 1000000 ))

    # Run GCC binary
    local r_g_start=$(date +%s%N)
    if ! timeout $TIMEOUT "$bin_c" </dev/null >/dev/null 2>&1; then
        echo "$bench:GRFAIL:$c_s_ms:$c_g_ms:$r_s_ms:0"
        return
    fi
    local r_g_end=$(date +%s%N)
    local r_g_ms=$(( (r_g_end - r_g_start) / 1000000 ))

    echo "$bench:PASS:$c_s_ms:$c_g_ms:$r_s_ms:$r_g_ms"
}
export -f run_bench
export COMPILER BENCHDIR TMPDIR TIMEOUT

# Find benchmarks (only those with matching .c file)
files=$(for f in "$BENCHDIR"/*.fs; do
    b=$(basename "$f" .fs)
    [[ -f "$BENCHDIR/$b.c" ]] && echo "$f"
done | grep -v -E '(MISSING|CRITERIA|run|benchmark)\.fs$')

count=$(echo "$files" | wc -l)
echo "Running $count benchmarks..."

# Run in parallel
touch "$RESULTS"
start=$(date +%s%3N)
if command -v parallel >/dev/null 2>&1; then
    echo "$files" | parallel -j$(nproc) run_bench {} >> "$RESULTS"
else
    for f in $files; do run_bench "$f" >> "$RESULTS"; done
fi
end=$(date +%s%3N)
wall=$((end - start))

# Parse results
results=$(cat "$RESULTS")
passed=$(echo "$results" | grep -c ':PASS:' || true)
cfail=$(echo "$results" | grep -c ':CFAIL:' || true)
rfail=$(echo "$results" | grep -c ':RFAIL:' || true)

# Aggregate timing
sum_cs=0 sum_cg=0 sum_rs=0 sum_rg=0
sixth_wins=0
while IFS=: read -r name status cs cg rs rg; do
    [[ "$status" != "PASS" ]] && continue
    sum_cs=$((sum_cs + cs))
    sum_cg=$((sum_cg + cg))
    sum_rs=$((sum_rs + rs))
    sum_rg=$((sum_rg + rg))
    [[ $rs -lt $rg ]] && ((sixth_wins++)) || true
done <<< "$results"

# Report
echo
echo "═══════════════════════════════════════════════════════════════"
echo "                    SIXTH vs GCC -O2 BENCHMARK"
echo "═══════════════════════════════════════════════════════════════"
echo
printf "%-20s %8s %8s %8s\n" "" "Sixth" "GCC -O2" "Ratio"
echo "───────────────────────────────────────────────────────────────"
printf "%-20s %7dms %7dms %7.2fx\n" "Compile (total)" "$sum_cs" "$sum_cg" "$(echo "scale=2; $sum_cg / $sum_cs" | bc)"
printf "%-20s %7dms %7dms %7.2fx\n" "Runtime (total)" "$sum_rs" "$sum_rg" "$(echo "scale=2; $sum_rs / $sum_rg" | bc)"
echo "───────────────────────────────────────────────────────────────"
echo
echo "Passed: $passed / $count"
echo "Compile failures: $cfail"
echo "Runtime failures: $rfail"
echo "Sixth faster runtime: $sixth_wins / $passed"
echo "Wall time: ${wall}ms (parallel)"
echo

# Show worst runtime ratios
if [[ "$passed" -gt 0 ]]; then
    echo "Worst runtime ratios (Sixth/GCC):"
    echo "$results" | grep ':PASS:' | while IFS=: read -r name status cs cg rs rg; do
        [[ $rg -gt 0 ]] && ratio=$(echo "scale=1; $rs / $rg" | bc) || ratio="inf"
        echo "$name $ratio"
    done | sort -t' ' -k2 -rn | head -5 | while read name ratio; do
        printf "  %-25s %6sx\n" "$name" "$ratio"
    done
fi

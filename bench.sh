#!/bin/bash
# bench.sh - sixth.fs vs gcc -O0/-O2. Median of 5 runs.
# gcc -O2 eliminates loops with no side effects (constant propagation).
# gcc -O0 gives the honest loop-execution comparison.
set -euo pipefail

RUNS=5
TMP=/tmp/fifth-bench
rm -rf "$TMP" && mkdir -p "$TMP"

# Benchmarks that both sixth.fs and forth2c+gcc can compile.
BOTH="collatz fib-std arith-std loop-std branch nested stack call fibrec"
TF_ONLY="fib arith loop spill mem"

die() { echo "FATAL: $1" >&2; exit 1; }

# Phase 1: Compile everything
echo "Compiling..."
for b in $BOTH $TF_ONLY; do
    ./sixth compiler/sixth.fs "bench/${b}.fs" "$TMP/tf_${b}" 2>/dev/null \
        || die "sixth.fs failed on $b"
    chmod +x "$TMP/tf_${b}"
done
for b in $BOTH; do
    python3 compiler/forth2c.py "bench/${b}.fs" "$TMP/${b}.c" 2>/dev/null \
        || die "forth2c failed on $b"
    gcc -O0 -o "$TMP/gcc0_${b}" "$TMP/${b}.c" 2>/dev/null \
        || die "gcc -O0 failed on $b"
    gcc -O2 -o "$TMP/gcc2_${b}" "$TMP/${b}.c" 2>/dev/null \
        || die "gcc -O2 failed on $b"
done
echo "Done."
echo ""

# Phase 2: Time
median() {
    local bin="$1"
    local times=()
    for ((r=0; r<RUNS; r++)); do
        local t0=$(date +%s%N)
        timeout 30 "$bin" >/dev/null 2>&1
        local t1=$(date +%s%N)
        times+=( $(( (t1 - t0) / 1000000 )) )
    done
    IFS=$'\n' sorted=($(sort -n <<<"${times[*]}")); unset IFS
    echo "${sorted[$((RUNS/2))]}"
}

ratio() {
    local tf="$1" gcc="$2"
    if [ "$gcc" -gt 0 ]; then
        awk "BEGIN{printf \"%.2fx\", $tf/$gcc}"
    else
        echo "n/a"
    fi
}

printf "%-15s %8s %8s %8s %8s %8s\n" "BENCHMARK" "tf(ms)" "gcc0(ms)" "gcc2(ms)" "tf/gcc0" "tf/gcc2"
printf "%-15s %8s %8s %8s %8s %8s\n" "---------" "------" "--------" "--------" "-------" "-------"

for b in $BOTH; do
    tf_ms=$(median "$TMP/tf_${b}")
    g0_ms=$(median "$TMP/gcc0_${b}")
    g2_ms=$(median "$TMP/gcc2_${b}")
    r0=$(ratio "$tf_ms" "$g0_ms")
    r2=$(ratio "$tf_ms" "$g2_ms")
    printf "%-15s %8d %8d %8d %8s %8s\n" "$b" "$tf_ms" "$g0_ms" "$g2_ms" "$r0" "$r2"
done

echo ""
printf "%-15s %8s %8s\n" "TF-ONLY" "tf(ms)" "notes"
printf "%-15s %8s %8s\n" "-------" "------" "-----"
for b in $TF_ONLY; do
    tf_ms=$(median "$TMP/tf_${b}")
    printf "%-15s %8d  %s\n" "$b" "$tf_ms" "non-std words"
done

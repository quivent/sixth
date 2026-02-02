#!/bin/bash
# Full benchmark: sixth.fs vs gcc -O0/-O1/-O2/-O3
# Outputs markdown table to stdout and bench/BENCHMARKS.md

set -euo pipefail

PROJECT="$(cd "$(dirname "$0")/.." && pwd)"
TF="$PROJECT/compiler/sixth.fs"
FIFTH="$PROJECT/fifth"
BENCHC="$PROJECT/bench/bench.c"
TMPDIR=$(mktemp -d)
trap "rm -rf $TMPDIR" EXIT

# Benchmarks: name, .fs file, C test name, expected output pattern
BENCHMARKS=(
    "arith|bench/arith.fs|arith|nos+ 1-nzloop (custom words)"
    "arith-std|bench/arith-std.fs|arith-std|swap 1+ swap / begin..while (standard)"
    "loop|bench/loop.fs|loop|1-nzloop (custom word)"
    "loop-std|bench/loop-std.fs|loop-std|1- dup 0> while repeat (standard)"
    "fib|bench/fib.fs|fib|tuck+ in do/loop (custom word)"
    "fib-std|bench/fib-std.fs|fib-std|swap over + in do/loop (standard)"
    "branch|bench/branch.fs|branch|100M if/else with 1 and"
    "stack|bench/stack.fs|stack|100M swaps"
    "nested|bench/nested.fs|nested|10K x 10K do/loop (100M)"
    "mem|bench/mem.fs|mem|100K x 1K memory write/read"
    "call|bench/call.fs|call|10M recursive countdown"
    "collatz|bench/collatz.fs|collatz|50K Collatz sequences"
    "spill|bench/spill.fs|spill|4-var rotate via memory"
    "arith50m|bench/arith50m.fs|arith50m|50M mixed ALU pipeline"
    "call100m|bench/call100m.fs|call100m|100M function calls"
    "fib38|bench/fib38.fs|fib38|recursive fib(38)"
    "nested100k|bench/nested100k.fs|nested100k|100K x 10K do/loop"
)

# Median of 3 runs (in ms)
median3() {
    local cmd="$1"
    local times=()
    for i in 1 2 3; do
        local t0=$(date +%s%N)
        eval "$cmd" >/dev/null 2>&1 || true
        local t1=$(date +%s%N)
        times+=( $(( (t1 - t0) / 1000000 )) )
    done
    # Sort and take middle
    IFS=$'\n' sorted=($(sort -n <<<"${times[*]}")); unset IFS
    echo "${sorted[1]}"
}

echo "Compiling C benchmarks at all optimization levels..."
gcc -O0 "$BENCHC" -o "$TMPDIR/bench_O0" 2>/dev/null
gcc -O1 "$BENCHC" -o "$TMPDIR/bench_O1" 2>/dev/null
gcc -O2 "$BENCHC" -o "$TMPDIR/bench_O2" 2>/dev/null
gcc -O3 "$BENCHC" -o "$TMPDIR/bench_O3" 2>/dev/null
echo "Done."

echo "Compiling and running benchmarks..."
echo

# Header
HDR="| Benchmark | Pattern | tf compile | tf run | gcc -O0 | gcc -O1 | gcc -O2 | gcc -O3 | tf/gcc-O2 | Correct |"
SEP="|-----------|---------|-----------|--------|---------|---------|---------|---------|-----------|---------|"

LINES=()
LINES+=("$HDR")
LINES+=("$SEP")

for entry in "${BENCHMARKS[@]}"; do
    IFS='|' read -r name fsfile cname desc <<< "$entry"

    # Compile with tf
    tf_compile_ms="-"
    tf_run_ms="-"
    tf_output=""
    outbin="$TMPDIR/tf_$name"

    t0=$(date +%s%N)
    if $FIFTH $TF "$PROJECT/$fsfile" "$outbin" 2>/dev/null; then
        t1=$(date +%s%N)
        tf_compile_ms=$(( (t1 - t0) / 1000000 ))
        chmod +x "$outbin" 2>/dev/null || true

        # Run tf (median of 3)
        tf_run_ms=$(median3 "$outbin")
        tf_output=$("$outbin" 2>/dev/null | head -1 | tr -d '[:space:]') || tf_output="ERROR"
    else
        t1=$(date +%s%N)
        tf_compile_ms="FAIL"
    fi

    # Run gcc versions (median of 3)
    gcc_o0="-"; gcc_o1="-"; gcc_o2="-"; gcc_o3="-"
    gcc_output=""

    if [ -x "$TMPDIR/bench_O0" ]; then
        gcc_o0=$(median3 "$TMPDIR/bench_O0 $cname")
        gcc_output=$("$TMPDIR/bench_O0" "$cname" 2>/dev/null | head -1 | tr -d '[:space:]') || gcc_output="ERROR"
    fi
    if [ -x "$TMPDIR/bench_O1" ]; then
        gcc_o1=$(median3 "$TMPDIR/bench_O1 $cname")
    fi
    if [ -x "$TMPDIR/bench_O2" ]; then
        gcc_o2=$(median3 "$TMPDIR/bench_O2 $cname")
    fi
    if [ -x "$TMPDIR/bench_O3" ]; then
        gcc_o3=$(median3 "$TMPDIR/bench_O3 $cname")
    fi

    # Ratio
    ratio="-"
    if [ "$tf_run_ms" != "-" ] && [ "$tf_run_ms" != "FAIL" ] && \
       [ "$gcc_o2" != "-" ] && [ "$gcc_o2" -gt 0 ] 2>/dev/null; then
        ratio_100=$(( tf_run_ms * 100 / gcc_o2 ))
        ratio_whole=$(( ratio_100 / 100 ))
        ratio_frac=$(( ratio_100 % 100 ))
        ratio=$(printf "%d.%02dx" $ratio_whole $ratio_frac)
    elif [ "$tf_run_ms" = "0" ] || [ "$gcc_o2" = "0" ] 2>/dev/null; then
        ratio="<1ms"
    fi

    # Correctness
    correct="?"
    if [ -n "$tf_output" ] && [ -n "$gcc_output" ]; then
        if [ "$tf_output" = "$gcc_output" ]; then
            correct="YES"
        elif [ "$tf_output" = "ERROR" ]; then
            correct="TF-FAIL"
        elif [ "$gcc_output" = "ERROR" ]; then
            correct="GCC-FAIL"
        else
            correct="NO (tf=$tf_output gcc=$gcc_output)"
        fi
    fi

    line="| $name | $desc | ${tf_compile_ms}ms | ${tf_run_ms}ms | ${gcc_o0}ms | ${gcc_o1}ms | ${gcc_o2}ms | ${gcc_o3}ms | $ratio | $correct |"
    LINES+=("$line")

    # Progress
    printf "  %-15s tf=%sms  gcc-O2=%sms  ratio=%s  %s\n" \
        "$name" "$tf_run_ms" "$gcc_o2" "$ratio" "$correct" >&2
done

echo >&2
echo "Writing bench/BENCHMARKS.md..." >&2

{
    echo "# Sixth Compiler Benchmarks: sixth.fs vs gcc"
    echo ""
    echo "Generated: $(date -u '+%Y-%m-%d %H:%M UTC')"
    echo ""
    echo "All times in milliseconds. Median of 3 runs."
    echo "tf compile = time to compile .fs to native binary."
    echo "tf/gcc-O2 = runtime ratio (lower is better for tf, 1.00x = parity)."
    echo ""
    for line in "${LINES[@]}"; do
        echo "$line"
    done
    echo ""
    echo "## Key"
    echo ""
    echo "- **arith vs arith-std**: Same computation. arith uses \`nos+\`/\`1-nzloop\` (custom). arith-std uses \`swap 1+ swap\`/\`begin..while\` (standard)."
    echo "- **loop vs loop-std**: Same computation. loop uses \`1-nzloop\` (2 insns). loop-std uses \`1- dup 0> while repeat\` (22 insns)."
    echo "- **fib vs fib-std**: Same computation. fib uses \`tuck+\` (1 insn). fib-std uses \`swap over +\` (5+ insns)."
    echo "- **Correct**: YES = tf and gcc-O0 produce identical output."
    echo ""
    echo "## Fusion Gap"
    echo ""
    echo "The gap between custom-word and standard-Forth benchmarks shows the cost of"
    echo "unoptimized word sequences. The pending-swap optimization targets this gap."
} > "$PROJECT/bench/BENCHMARKS.md"

echo "Done. Results in bench/BENCHMARKS.md" >&2
echo "" >&2
cat "$PROJECT/bench/BENCHMARKS.md"

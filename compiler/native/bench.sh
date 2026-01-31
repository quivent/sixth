#!/bin/bash
# bench.sh - Fifth tf.fs vs GCC benchmark comparison
# Usage: ./bench.sh [N]  (N = number of benchmarks, 1-5, default 5)
# Uses /usr/bin/time for accurate measurement (no shell arithmetic in the hot path)

cd "$(dirname "$0")/../.."

N=${1:-5}
if [ "$N" -gt 5 ]; then N=5; fi
if [ "$N" -lt 1 ]; then N=1; fi

FIFTH="./fifth"
TF="compiler/tf.fs"
DIR="/tmp/fifth-bench"
rm -rf "$DIR"
mkdir -p "$DIR"

# --- Benchmark source files ---

cat > "$DIR/1_fib.fs" << 'EOF'
: fib ( n -- result ) dup 2 < if else dup 1- recurse swap 2 - recurse + then ;
: main ( -- ) 45 fib . cr ;
EOF
cat > "$DIR/1_fib.c" << 'EOF'
#include <stdio.h>
long fib(long n) { if (n < 2) return n; return fib(n-1) + fib(n-2); }
int main() { printf("%ld\n", fib(45)); return 0; }
EOF

cat > "$DIR/2_sum.fs" << 'EOF'
: main ( -- )
  0 1000000000
  begin dup while
    swap over + swap 1-
  repeat drop . cr ;
EOF
cat > "$DIR/2_sum.c" << 'EOF'
#include <stdio.h>
#include <stdint.h>
int main() {
    int64_t sum = 0;
    for (int64_t i = 1; i <= 1000000000LL; i++) sum += i;
    printf("%ld\n", sum);
    return 0;
}
EOF

cat > "$DIR/3_loop.fs" << 'EOF'
: main ( -- )
  1000000000 begin 1- dup while repeat drop 0 . cr ;
EOF
cat > "$DIR/3_loop.c" << 'EOF'
#include <stdio.h>
#include <stdint.h>
int main() {
    for (volatile int64_t i = 1000000000LL; i > 0; i--);
    printf("0\n");
    return 0;
}
EOF

cat > "$DIR/4_doloop.fs" << 'EOF'
: main ( -- )
  1000000000 0 do loop 0 . cr ;
EOF
cat > "$DIR/4_doloop.c" << 'EOF'
#include <stdio.h>
#include <stdint.h>
int main() {
    for (volatile int64_t i = 0; i < 1000000000LL; i++);
    printf("0\n");
    return 0;
}
EOF

cat > "$DIR/5_arith.fs" << 'EOF'
: main ( -- )
  1 100000000
  begin dup while
    swap 3 * 7 + $FFFFFF and swap 1-
  repeat drop . cr ;
EOF
cat > "$DIR/5_arith.c" << 'EOF'
#include <stdio.h>
#include <stdint.h>
int main() {
    int64_t x = 1;
    for (int64_t i = 100000000; i > 0; i--)
        x = (x * 3 + 7) & 0xFFFFFF;
    printf("%ld\n", x);
    return 0;
}
EOF

BENCH_NAMES=("fib(45)rec" "sum(1B)" "loop(1B)" "do-loop(1B)" "arith(100M)")
BENCH_IDS=(1_fib 2_sum 3_loop 4_doloop 5_arith)

# --- Measure with /usr/bin/time (writes seconds to stderr) ---
# Returns seconds as string in $T, captures stdout in $OUT
measure() {
    local timefile="$DIR/.time"
    OUT=$(/usr/bin/time -f '%e' -o "$timefile" timeout 120 "$@" 2>/dev/null) || OUT="FAIL"
    T=$(cat "$timefile" 2>/dev/null || echo "0.00")
}

# --- Run benchmarks ---

echo ""
echo "Fifth Native Compiler (tf.fs) vs GCC"
echo "======================================"
echo ""

# Header
printf "%-13s │  %-13s │  %-13s │  %-13s │  %-13s │  %-13s\n" \
    "" "tf.fs" "gcc -O0" "gcc -O1" "gcc -O2" "gcc -O3"
printf "%-13s │ %6s %6s │ %6s %6s │ %6s %6s │ %6s %6s │ %6s %6s\n" \
    "benchmark" "comp" "run" "comp" "run" "comp" "run" "comp" "run" "comp" "run"
echo "──────────────┼──────────────┼──────────────┼──────────────┼──────────────┼──────────────"

MISMATCHES=""

for ((idx=0; idx<N; idx++)); do
    id="${BENCH_IDS[$idx]}"
    desc="${BENCH_NAMES[$idx]}"
    fs="$DIR/${id}.fs"
    c="$DIR/${id}.c"

    # --- tf.fs compile ---
    tf_bin="$DIR/${id}_tf"
    measure $FIFTH $TF "$fs" "$tf_bin"
    tf_comp="$T"

    # --- tf.fs run ---
    if [ -f "$tf_bin" ]; then
        measure "$tf_bin"
        tf_run="$T"; tf_out="$OUT"
    else
        tf_run="FAIL"; tf_out=""
    fi

    # --- GCC compile + run at each -O level ---
    gc0="" gc1="" gc2="" gc3=""
    gr0="" gr1="" gr2="" gr3=""
    go0="" go1="" go2="" go3=""
    for opt in 0 1 2 3; do
        gcc_bin="$DIR/${id}_gcc_O${opt}"
        measure gcc -O${opt} -o "$gcc_bin" "$c"
        eval "gc${opt}=\"\$T\""
        measure "$gcc_bin"
        eval "gr${opt}=\"\$T\""
        eval "go${opt}=\"\$OUT\""
    done

    # --- Print row ---
    printf "%-13s │ %6s %6s │ %6s %6s │ %6s %6s │ %6s %6s │ %6s %6s\n" \
        "$desc" \
        "${tf_comp}s" "${tf_run}s" \
        "${gc0}s" "${gr0}s" \
        "${gc1}s" "${gr1}s" \
        "${gc2}s" "${gr2}s" \
        "${gc3}s" "${gr3}s"

    # --- Check output ---
    tf_trimmed=$(echo "$tf_out" | tr -d ' \n')
    gcc_trimmed=$(echo "$go0" | tr -d ' \n')
    if [ -n "$tf_trimmed" ] && [ "$tf_trimmed" != "$gcc_trimmed" ]; then
        MISMATCHES="${MISMATCHES}  ${desc}: tf=${tf_trimmed} gcc=${gcc_trimmed}\n"
    fi
done

echo ""

if [ -n "$MISMATCHES" ]; then
    echo "OUTPUT MISMATCHES (tf vs gcc):"
    printf "$MISMATCHES"
    echo ""
fi

echo "All times via /usr/bin/time (wall-clock seconds)."
echo ""

rm -rf "$DIR"

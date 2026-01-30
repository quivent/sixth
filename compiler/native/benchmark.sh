#!/bin/bash
# Benchmark Fifth native compiler vs C

cd "$(dirname "$0")"
FIFTH="../../fifth"

echo "=== Building Fifth binaries ==="
$FIFTH loop10b.fs
$FIFTH sum1b.fs
$FIFTH fib45.fs

echo ""
echo "=== Building C binaries (gcc -O0) ==="
gcc -O0 -o loop10b_c_O0 loop10b.c
gcc -O0 -o sum1b_c_O0 sum1b.c
gcc -O0 -o fib45_c_O0 fib45.c

echo ""
echo "=== Building C binaries (gcc -O2) ==="
gcc -O2 -o loop10b_c_O2 loop10b.c
gcc -O2 -o sum1b_c_O2 sum1b.c
gcc -O2 -o fib45_c_O2 fib45.c

echo ""
echo "=== Building C binaries (gcc -O3) ==="
gcc -O3 -o loop10b_c_O3 loop10b.c
gcc -O3 -o sum1b_c_O3 sum1b.c
gcc -O3 -o fib45_c_O3 fib45.c

echo ""
echo "=== Binary sizes ==="
echo "Fifth:"
ls -la loop10b sum1b fib45 2>/dev/null | awk '{print $5, $9}'
echo ""
echo "C -O2:"
ls -la loop10b_c_O2 sum1b_c_O2 fib45_c_O2 2>/dev/null | awk '{print $5, $9}'

echo ""
echo "========================================="
echo "BENCHMARK: 10 billion loop iterations"
echo "========================================="
echo "Fifth:"
time ./loop10b
echo ""
echo "C -O0:"
time ./loop10b_c_O0
echo ""
echo "C -O2:"
time ./loop10b_c_O2

echo ""
echo "========================================="
echo "BENCHMARK: Sum 1 to 1 billion"
echo "========================================="
echo "Fifth:"
time ./sum1b
echo ""
echo "C -O0:"
time ./sum1b_c_O0
echo ""
echo "C -O2:"
time ./sum1b_c_O2

echo ""
echo "========================================="
echo "BENCHMARK: Recursive Fibonacci(45)"
echo "========================================="
echo "Fifth:"
time ./fib45
echo ""
echo "C -O0:"
time ./fib45_c_O0
echo ""
echo "C -O2:"
time ./fib45_c_O2

echo ""
echo "=== Cleanup ==="
rm -f loop10b_c_O0 loop10b_c_O2 loop10b_c_O3
rm -f sum1b_c_O0 sum1b_c_O2 sum1b_c_O3
rm -f fib45_c_O0 fib45_c_O2 fib45_c_O3
echo "Done."

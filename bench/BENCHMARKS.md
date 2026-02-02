# Sixth Compiler Benchmarks: sixth.fs vs gcc

Generated: 2026-02-01 17:43 UTC

All times in milliseconds. Median of 3 runs.
tf compile = time to compile .fs to native binary.
tf/gcc-O2 = runtime ratio (lower is better for tf, 1.00x = parity).

| Benchmark | Pattern | tf compile | tf run | gcc -O0 | gcc -O1 | gcc -O2 | gcc -O3 | tf/gcc-O2 | Correct |
|-----------|---------|-----------|--------|---------|---------|---------|---------|-----------|---------|
| arith | nos+ 1-nzloop (custom words) | 8ms | 0ms | 509ms | 218ms | 1ms | 1ms | 0.00x | YES |
| arith-std | swap 1+ swap / begin..while (standard) | 8ms | 1ms | 507ms | 223ms | 1ms | 1ms | 1.00x | YES |
| loop | 1-nzloop (custom word) | 9ms | 1ms | 382ms | 209ms | 1ms | 1ms | 1.00x | YES |
| loop-std | 1- dup 0> while repeat (standard) | 8ms | 0ms | 382ms | 224ms | 1ms | 1ms | 0.00x | YES |
| fib | tuck+ in do/loop (custom word) | 8ms | 218ms | 925ms | 432ms | 234ms | 267ms | 0.93x | YES |
| fib-std | swap over + in do/loop (standard) | 8ms | 331ms | 881ms | 459ms | 231ms | 232ms | 1.43x | YES |
| branch | 100M if/else with 1 and | 8ms | 91ms | 69ms | 42ms | 43ms | 47ms | 2.11x | YES |
| stack | 100M swaps | 9ms | 30ms | 70ms | 22ms | 22ms | 22ms | 1.36x | YES |
| nested | 10K x 10K do/loop (100M) | 8ms | 22ms | 51ms | 24ms | 1ms | 1ms | 22.00x | YES |
| mem | 100K x 1K memory write/read | 9ms | 121ms | 77ms | 23ms | 11ms | 1ms | 11.00x | YES |
| call | 10M recursive countdown | 8ms | 5ms | 5ms | 3ms | 1ms | 1ms | 5.00x | YES |
| collatz | 50K Collatz sequences | 8ms | 14ms | 12ms | 6ms | 4ms | 5ms | 3.50x | YES |
| spill | 4-var rotate via memory | 9ms | 1086ms | 138ms | 60ms | 60ms | 64ms | 18.10x | YES |
| arith50m | 50M mixed ALU pipeline | 12ms | 250ms | 168ms | 147ms | 110ms | 116ms | 2.27x | YES |
| call100m | 100M function calls | 8ms | 102ms | 170ms | 108ms | 108ms | 105ms | 0.94x | YES |
| fib38 | recursive fib(38) | 9ms | 121ms | 157ms | 137ms | 76ms | 73ms | 1.59x | YES |
| nested100k | 100K x 10K do/loop | 9ms | 244ms | 545ms | 199ms | 1ms | 1ms | 244.00x | YES |

## Key

- **arith vs arith-std**: Same computation. arith uses `nos+`/`1-nzloop` (custom). arith-std uses `swap 1+ swap`/`begin..while` (standard).
- **loop vs loop-std**: Same computation. loop uses `1-nzloop` (2 insns). loop-std uses `1- dup 0> while repeat` (22 insns).
- **fib vs fib-std**: Same computation. fib uses `tuck+` (1 insn). fib-std uses `swap over +` (5+ insns).
- **Correct**: YES = tf and gcc-O0 produce identical output.

## Fusion Gap

The gap between custom-word and standard-Forth benchmarks shows the cost of
unoptimized word sequences. The pending-swap optimization targets this gap.

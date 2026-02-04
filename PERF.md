# Sixth Performance Reference

## Status

Sixth matches GCC -O1 on tight loops. 1.7-4.5x slower than GCC -O2 on real workloads.

| Benchmark | Sixth | GCC -O2 | Ratio |
|-----------|-------|---------|-------|
| ack(3,10) | 54ms | 12ms | 4.5x slower |
| ack(4,1) | 3.48s | 0.77s | 4.5x slower |
| fib40 | 319ms | 101ms | 3.2x slower |
| primes(10000) | 5ms | 3ms | 1.7x slower |

## Loop Comparison

```asm
# Sixth 1-nzloop (2 instructions, matches GCC -O1)
loop:
  dec rax
  jne loop

# GCC -O1 (2 instructions)
loop:
  sub $1, %eax
  jne loop
```

## Architecture

```
rax = TOS
rbx = NOS (depth >= 2)
rcx = third (depth >= 3)
r15 = data stack pointer (grows down)
r12 = do/loop index
r13 = do/loop limit
```

## Commands

```bash
# Core benchmarks (Sixth)
./engine/fifth compiler/sixth.fs compiler/bench/ack.fs /tmp/ack && time /tmp/ack
./engine/fifth compiler/sixth.fs compiler/bench/fib40.fs /tmp/fib && time /tmp/fib
./engine/fifth compiler/sixth.fs compiler/bench/primes.fs /tmp/primes && time /tmp/primes

# Compare to GCC -O2
gcc -O2 compiler/bench/ack.c -o /tmp/ack_c && time /tmp/ack_c
gcc -O2 compiler/bench/fib40.c -o /tmp/fib_c && time /tmp/fib_c
gcc -O2 compiler/bench/primes.c -o /tmp/primes_c && time /tmp/primes_c
```

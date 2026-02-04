# Benchmark Coverage

## Missing Compiler Words

These words are used by benchmarks but may not be in the compiler:

| Word | Status | Notes |
|------|--------|-------|
| `d.` | **MISSING** | Workaround: `drop .` (loses high cell) |

Benchmark double.fs now uses `drop .` instead of `d.`

### Required Vocabulary (all benchmarks)

```
Stack:    dup drop swap over nip rot pick 2dup
Arith:    + - * / mod and lshift rshift 1+ 1- 2/
Compare:  < > = 0= 0> >=
Memory:   @ ! c@ c! cells
Control:  if then else begin while repeat do loop i j exit recurse
Define:   : ; constant variable create allot
Double:   d+ 0. 1.
I/O:      . cr emit
```

Check `compile-builtin` in sixth.fs for implementation status.

---

## Primitive Isolation (new)

| Test | Forth | C | Stresses |
|------|-------|---|----------|
| arith | arith.fs | arith.c | Pure arithmetic |
| mem | mem.fs | mem.c | Pure memory access |
| ctrl | ctrl.fs | ctrl.c | Pure control flow |
| call | call.fs | call.c | Call overhead |
| shift | shift.fs | shift.c | Shift operations |
| double | double.fs | double.c | Double-cell arithmetic |

## Mixed Workloads (existing)

| Test | Forth | C | Stresses |
|------|-------|---|----------|
| fib40 | fib40.fs | fib40.c | Recursion |
| ack | ack.fs | ack.c | Deep recursion |
| tak | tak.fs | tak.c | Heavy recursion |
| collatz | collatz.fs | collatz.c | Conditionals + arithmetic |
| primes | primes.fs | primes.c | Division + conditionals |
| sieve1m | sieve1m.fs | sieve1m.c | Byte memory + loops |
| matmul | matmul.fs | matmul.c | Strided memory |
| mandel | mandel.fs | — | I/O heavy, skip |

## Method

```bash
# Compile C
gcc -O2 bench.c -o bench_c

# Compile Sixth (when ready)
./engine/fifth bench.fs -o bench_fs

# Compare
time ./bench_c
time ./bench_fs
```

Loser reveals weakness. Fix. Repeat.

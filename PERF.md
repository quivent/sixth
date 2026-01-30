# Fifth Native Compiler Performance Log

## Goal

Compile Fifth directly to x86_64 machine code. No C. No assembler. Forth to binary.

Measure against C compilers. Match them. Beat them.

## Results

| Date | Compiler | Time (s) | Loop Instructions | Notes |
|------|----------|----------|-------------------|-------|
| 2026-01-30 | GCC -O0 | 0.04 | 5 | Memory load/store every iteration |
| 2026-01-30 | Fifth (original) | 0.04 | 12 | `dup 0= until` pattern |
| 2026-01-30 | Fifth (nzloop) | 0.02 | 3 | `test rax; jnz` |
| 2026-01-30 | Fifth (1-nzloop) | 0.02 | 2 | `dec rax; jnz` |
| 2026-01-30 | GCC -O1 | 0.02 | 2 | `sub eax; jne` |
| 2026-01-30 | GCC -O2 | 0.00 | 0 | Loop eliminated entirely |

**Status: PARITY WITH GCC -O1** (tight loops)

## Full Benchmark Suite

| Compiler | Time | vs GCC -O1 |
|----------|------|------------|
| Fifth | 0.083s | 2.4x slower |
| Fifth +<if | 0.077s | 2.2x slower |
| GCC -O0 | 0.106s | 3x slower |
| GCC -O1 | 0.035s | baseline |
| GCC -O2 | 0.011s | 3x faster |

**Fifth beats GCC -O0 on full suite.**

## Breakdown by Benchmark

| Benchmark | Fifth | Fifth +<if | GCC -O1 | Status |
|-----------|-------|------------|---------|--------|
| Loop | 0.021s | 0.021s | 0.021s | PARITY |
| Fib | 0.044s | 0.035s | 0.029s | 20% slower |
| Nested | 0.001s | 0.001s | 0.001s | PARITY |

## Optimizations Applied

1. **`<if` word** - Combines `< if` into single compare+branch, no flag conversion
   - Fib speedup: 0.044s → 0.035s (20% faster)
   - Uses `jge` directly instead of setl/movzx/neg/test/jnz

## Benchmark: Fibonacci(35)

| Compiler | Time (s) | Notes |
|----------|----------|-------|
| Fifth | 0.044 | Call overhead |
| GCC -O0 | 0.035 | baseline |
| GCC -O1 | 0.029 | |
| GCC -O2 | 0.011 | Optimized |

Fifth is 25% slower than GCC -O0 on recursive code. Call overhead dominates.

## Benchmark

Count down from 100 million.

```forth
\ Original (12 instructions in loop)
: main 100000000 begin 1- dup 0= until drop ;

\ Optimized (3 instructions)
: main 100000000 begin 1- nzloop drop ;

\ Maximum (2 instructions)
: main 100000000 begin 1-nzloop drop ;
```

```c
// C equivalent
int main() {
    int n = 100000000;
    while (n--);
    return 0;
}
```

## Generated Code Comparison

### Fifth original (12 instructions)
```asm
loop:
  dec rax              ; 1-
  sub r15, 8           ; dup
  mov [r15], rax
  test rax, rax        ; 0=
  sete al
  movzx rax, al
  neg rax
  mov rcx, rax         ; until
  mov rax, [r15]
  add r15, 8
  test rcx, rcx
  jz loop
```

### Fifth nzloop (3 instructions)
```asm
loop:
  dec rax              ; 1-
  test rax, rax        ; nzloop
  jne loop
```

### Fifth 1-nzloop (2 instructions)
```asm
loop:
  dec rax              ; 1-nzloop (dec sets ZF)
  jne loop
```

### GCC -O1 (2 instructions)
```asm
loop:
  sub $1, %eax
  jne loop
```

Identical hot path.

## New Optimization Words

| Word | Stack | Loop Body | Description |
|------|-------|-----------|-------------|
| `nzloop` | ( n -- n ) | 3 inst | Loop while TOS non-zero, keep TOS |
| `1-nzloop` | ( n -- n-1 ) | 2 inst | Decrement and loop, dec sets flags |
| `0=until` | ( n -- ) | 5 inst | Exit when TOS=0, consumes TOS |

## Why Fifth Beats GCC -O0

1. **TOS caching** - Top of stack lives in rax, not memory
2. **No frame pointer** - No push rbp / mov rbp, rsp overhead
3. **Register loops** - Counter stays in register, no load/store

GCC -O0 loads and stores to `[rbp-4]` every iteration. Fifth keeps everything in rax.

## Architecture

```
r15 = data stack pointer (grows down)
rax = TOS (top of stack, cached)
r14 = return stack pointer (for do/loop)
```

Stack operations:
- push: `sub r15, 8; mov [r15], rax`
- pop: `mov rax, [r15]; add r15, 8`

## Plan: Next Optimizations

### Phase 1: Peephole Patterns (Low-hanging fruit)

1. **`dup 0=`** → `test rax, rax; setz...` (don't push/pop)
2. **`1+ 1+`** → `add rax, 2`
3. **`drop drop`** → `add r15, 16; mov rax, [r15-8]`
4. **`swap drop`** → `mov rax, [r15]; add r15, 8`

### Phase 2: Control Flow

1. **Loop unrolling** - Unroll small constant loops
2. **Tail call optimization** - `jmp` instead of `call; ret`
3. **Branch prediction hints** - Likely/unlikely paths

### Phase 3: Constant Folding

1. **`3 4 +`** → `7` at compile time
2. **Dead code elimination** - Remove unreachable branches
3. **Strength reduction** - `2 *` → `shl rax, 1`

### Phase 4: Register Allocation

1. **Second TOS register** - Cache NOS in rbx
2. **Loop variables in registers** - `do...loop` index in register
3. **Spill analysis** - Minimize stack traffic

## To Beat GCC -O2

GCC -O2 eliminates the loop entirely. It computes that `n` ends at 0 and just returns.

To match this:
1. Detect pure countdown loops
2. Compute final value at compile time
3. Emit only the final state

This requires dataflow analysis. Significant complexity.

## Bug Fixes (2026-01-30)

1. **gen-if flag clobber** - `pop-tos` after `test` clobbers ZF. Fixed: save result to cl before pop.
2. **gen-until flag clobber** - Same issue. Fixed: save to rcx, pop, then test rcx.
3. **gen-while flag clobber** - Same fix.
4. **gen-rot dead code** - Stale code block executed. Fixed: removed.
5. **exit word missing** - Added `exit` → `ret`.

## Files

```
bench/loop.fs       - Loop countdown benchmark
bench/loop.c        - C equivalent
bench/loop-tight.fs - nzloop version
bench/loop-2inst.fs - 1-nzloop version (fastest)
bench/fib.fs        - Fibonacci benchmark (recursive)
bench/fib.c         - C equivalent
```

## Commands

```bash
# Compile and benchmark
cp bench/loop-2inst.fs input.fs
./fifth compiler/tf.fs
chmod +x output
time ./output

# Compare all versions
for f in bench/loop-fifth*; do echo "$f:"; time $f; done
```

# Fifth Native Compiler - Optimization Plan

## Current State

- tf.fs compiles Fifth to x86_64 ELF binaries
- TOS cached in rax
- Matches GCC -O1 on tight loops (2 instructions)
- Beats GCC -O0 on full benchmark (0.083s vs 0.106s)
- 2.4x slower than GCC -O1 on full benchmark (0.083s vs 0.035s)

## Target: Match GCC -O1

Gap is in recursion (fib). Call overhead dominates.

| Optimization | Expected Gain | LoC | Priority |
|--------------|---------------|-----|----------|
| Inline primitives | ~10ms | +50 | 1 |
| Faster call sequence | ~5ms | +20 | 2 |
| NOS caching (rbx) | ~5ms | +100 | 3 |

Compiler size: 995 lines, 32KB

## Immediate (This Week)

### 1. More Benchmarks - DONE

Fibonacci added. Results:
- Loop: Fifth matches GCC -O1 (2 instructions)
- Fib: Fifth is 25% slower than GCC -O0 (call overhead)

Next benchmarks:
```forth
\ Sum of array
: sum ( addr n -- sum )
  0 -rot 0 do over i + @ + loop nip ;

\ Prime sieve
: sieve ... ;
```

### 2. Peephole: dup + comparison

Pattern: `dup 0=`, `dup 0<`, `dup 0>`

Current: push, test, sete, movzx, neg
Better: test without push, generate flag in separate register

```forth
: gen-dup-0= ( -- )
  \ Keep rax, put flag in rcx
  $48 c, $85 c, $c0 c,     \ test rax, rax
  $0f c, $94 c, $c1 c,     \ setz cl
  $48 c, $0f c, $b6 c, $c9 c,  \ movzx rcx, cl
  $48 c, $f7 c, $d9 c,     \ neg rcx
  push-rcx ;               \ push rcx (flag), keep rax
```

### 3. Peephole: arithmetic chains

```
1+ 1+     → add rax, 2
1- 1-     → sub rax, 2
2* 2*     → shl rax, 2
dup +     → shl rax, 1
```

## Medium Term

### 4. Constant Folding

Track when TOS is a known constant. Fold at compile time.

```forth
3 4 +     \ Emit: mov rax, 7
2 3 * 1+  \ Emit: mov rax, 7
```

Requires: compile-time stack simulation (just track constants).

### 5. Tail Call Optimization

```forth
: foo ... bar ;   \ call bar; ret → jmp bar
```

Detect: last word before `;` is a call. Replace `call X; ret` with `jmp X`.

### 6. Inline Small Words

Words under 10 bytes: inline instead of call.

```forth
: double dup + ;  \ 6 bytes - inline it
```

Track word sizes during compilation. Inline threshold ~10 bytes.

## Longer Term

### 7. Register Allocation

Two-register TOS: rax = TOS, rbx = NOS

```
Current:         With NOS cache:
dup              mov rbx, rax
  sub r15, 8       (nothing - rbx already has it)
  mov [r15], rax

swap             xchg rax, rbx
  mov rcx, [r15]
  mov [r15], rax
  mov rax, rcx
```

Significant rewrite. Save for later.

### 8. Loop Analysis

Detect: `begin ... 1-nzloop` with constant start → compute iterations.
Unroll small loops. Eliminate trivial loops entirely.

GCC -O2 territory. Complex.

## Non-Goals

- Portability (x86_64 Linux only)
- Standards compliance (this is Fifth, not ANS Forth)
- Safety (no bounds checking, caller's problem)
- Large programs (fits in head or don't write it)

## Metrics

Track for each optimization:

1. Code size change
2. Benchmark time change
3. Complexity added to compiler

If complexity outweighs benefit, don't do it.

## Files to Modify

```
compiler/tf.fs     - Main compiler (add peephole patterns)
bench/*.fs         - More benchmarks
PERF.md            - Results log
```

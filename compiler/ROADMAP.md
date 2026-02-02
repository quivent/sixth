# sixth.fs Native Compiler Roadmap

## Current State

sixth.fs compiles Forth to x86-64 machine code. Single-pass, ~2600 lines of Forth.

### Vocabulary Coverage

| Source | Words | Notes |
|--------|-------|-------|
| sixth.fs compiled | 105 | 93 standard + 12 custom |
| ANS Forth Core | ~180 | sixth.fs covers ~58% |
| Sixth interpreter | ~480 | 178 C prims + 15 boot + 302 lib |

### Compiled Words

**Arithmetic**: `+` `-` `*` `/` `mod` `/mod` `negate` `abs` `1+` `1-` `2+` `2-` `2*` `2/`
**Comparison**: `=` `<>` `<` `>` `0=` `0<` `0<>` `<=` `>=` `u<` `u>` `within`
**Logic**: `and` `or` `xor` `invert`
**Stack**: `dup` `drop` `swap` `over` `rot` `nip` `tuck` `2dup` `2drop` `2swap` `2over` `?dup` `depth` `pick`
**Control**: `if` `else` `then` `begin` `until` `begin` `while` `repeat` `do` `loop` `+loop` `i` `j` `leave` `recurse` `recursive` `exit`
**Memory**: `@` `!` `c@` `c!` `+!` `cells` `cell+` `variable` `constant` `create` `allot` `here` `,` `c,`
**Return Stack**: `>r` `r>` `r@` `2>r` `2r>` `2r@`
**I/O**: `.` `emit` `cr` `type` `." ..."` `s"` `count`
**Extra Ops**: `min` `max` `lshift` `rshift`
**Double-Cell**: `s>d` `um*` `m*` `um/mod` `sm/rem` `fm/mod` `d+` `d-`
**Custom**: `nos+` `nos-` `tuck+` `dup+` `dup-` `<if` `1-nzloop` `dup2` `0<if` `0=if` `0<>if`

### Reference Benchmark: Ackermann

**ack.fs is the source of truth.** If ack fails, investigate:
1. Is it an optimization bug? Disable optimizations to isolate.
2. Is it a code generation bug? Compare with gcc -O2 output.

Do not modify ack.fs to make it pass. Fix the compiler.

```bash
./sixth compiler/sixth.fs compiler/bench/ack.fs /tmp/ack && /tmp/ack
# Expected: 8189
```

### Per-Optimization Anchors (sustain these)

| Optimization | Key Test | sixth/gcc-O2 | Floor |
|-------------|----------|--------------|-------|
| Stack caching (TOS in rax) | 100-dup-add | 1.48x | > 1.2x |
| Stack caching | 08-swap | 1.44x | > 1.2x |
| Superinstruction `dup+` | 100-dup-add | 1.48x | > 1.2x |
| Branch fusion `<if` | 450-dup-gt-while | 1.07x | > 0.9x |
| do/loop registers (r12/r13) | 614-doloop-basic | 1.17x | > 1.0x |
| Constant folding | 1002-fold-mul | 1.29x | > 1.0x |
| Literal-op fusion | 1019-fuse-and-imm | 1.31x | > 1.0x |
| Fusion in loop | 1047-fuse-in-loop | 1.32x | > 1.0x |
| Tail-call (recurse→jmp) | 235-recurse-fact | 1.04x | > 0.9x |
| Forward references | 1031-fwd-ref-chain | 1.20x | > 1.0x |

**Known weak spots** (slower than gcc-O2, investigate later):
- 320-factorial-5: 0.76x (non-tail recursion)
- 1000-palindrome: 0.79x (complex control flow)
- 1008-gcd-lcm: 0.82x (mutual recursion overhead)
- 1032-fwd-ref-mutual: 0.87x (double-pass penalty)

### Completed Phases

| Phase | Description | Status |
|-------|-------------|--------|
| 1 | Return stack (`>r` `r>` `r@` `2>r` `2r>` `2r@`) | DONE |
| 2 | Memory and defining words (`variable` `constant` `@` `!` etc.) | DONE |
| 3 | Trivial operations (`min` `max` `lshift` `rshift` `within`) | DONE |
| 4 | Strings (`s"` `type` `count` `."`) | DONE |
| 5 | Double-cell and unsigned (`um*` `um/mod` `s>d` `d+` `d-`) | DONE |

---

## Self-Hosting Roadmap (9 Steps)

Goal: sixth.fs compiles itself. No C. No dependencies.

| Step | Feature | Lines | Status |
|------|---------|-------|--------|
| 1 | Inlining | ~50 | BLOCKED (dup-pending conflict) |
| 2 | Strength reduction | ~30 | DONE |
| 3 | Run benchmarks | - | DONE |
| 4 | Input parsing | ~100 | NOT STARTED |
| 5 | Dictionary lookup | ~60 | NOT STARTED |
| 6 | CREATE DOES> | ~80 | NOT STARTED |
| 7 | STATE [ ] | ~20 | NOT STARTED |
| 8 | ACCEPT | ~65 | NOT STARTED |
| 9 | Delete C | 0 | NOT STARTED |

**Completed: 2 of 9**

---

## Step 1: Inlining (BLOCKED)

Words under 20 bytes: copy code inline instead of `call`.

```forth
: 2*  2* ;   \ 3 bytes — inline it
: big-word  ... 200 bytes ... ;   \ call it
```

**Problem**: Conflicts with dup-pending optimization. When a word is inlined into a context with something else in rbx, wrong result.

**Status**: Disabled. Needs redesign of dup-pending to work with inlining.

---

## Step 2: Strength Reduction (DONE)

Pattern match on literals. Emit faster instructions.

```
8 *    →  3 lshift      (power of 2 only)
8 mod  →  7 and         (power of 2 only)
```

**Tests**: 2100-strength-mul-2.fs, 2101-strength-mul-8.fs, 2103-strength-no-3.fs, 2104-strength-runtime.fs

---

## Step 3: Run Benchmarks (DONE)

```bash
./sixth compiler/bench/run.fs
```

| Benchmark | Sixth | GCC -O2 | Ratio |
|-----------|-------|---------|-------|
| ack(3,10) | 53ms | 12ms | 4.4x slower |
| primes(10000) | 23ms | 14ms | 1.6x slower |

Sixth compiles instant. GCC compiles in 30ms. Binaries 12-26x smaller.

---

## Step 4: Input Parsing (~100 lines)

| Word | Stack | Purpose |
|------|-------|---------|
| `SOURCE` | `( -- addr u )` | Current input buffer |
| `>IN` | `( -- addr )` | Parse position variable |
| `WORD` | `( char -- c-addr )` | Parse delimited token |
| `PARSE` | `( char -- addr u )` | Parse to delimiter |
| `EVALUATE` | `( addr u -- )` | Interpret string |

---

## Step 5: Dictionary Lookup (~60 lines)

| Word | Stack | Purpose |
|------|-------|---------|
| `FIND` | `( c-addr -- c-addr 0 \| xt 1 \| xt -1 )` | Look up word |
| `'` | `( "name" -- xt )` | Parse and find |
| `[']` | Compile-time `'` | Compile xt as literal |
| `EXECUTE` | `( xt -- )` | Run xt |
| `>BODY` | `( xt -- addr )` | Data field of CREATE'd word |

---

## Step 6: CREATE DOES> (~80 lines)

```forth
: constant  create , does> @ ;
: variable  create 0 , ;
```

Enables user-defined defining words. The heart of Forth extensibility.

---

## Step 7: STATE [ ] (~20 lines)

| Word | Stack | Purpose |
|------|-------|---------|
| `STATE` | `( -- addr )` | 0=interpret, nonzero=compile |
| `[` | `( -- )` | Switch to interpret mode |
| `]` | `( -- )` | Switch to compile mode |

---

## Step 8: ACCEPT (~65 lines)

| Word | Lines | Purpose |
|------|-------|---------|
| `ACCEPT` | ~30 | Read line from user |
| `ALLOT` | ~5 | Already exists |
| `ALIGN ALIGNED` | ~15 | Memory alignment |
| `UNLOOP` | ~10 | Clean return stack on LEAVE |
| `HEX DECIMAL` | ~5 | Set base |

---

## Step 9: Delete C (0 lines)

Once steps 4-8 complete:

1. `./sixth sixth.fs -o sixth` — compile the compiler
2. `./sixth` — runs natively, interprets and compiles
3. `rm -rf engine/` — delete the C interpreter

No more C compiler dependency. Sixth compiles Sixth.

---

## Line Count Projection

| Phase | Lines | Running Total |
|-------|-------|---------------|
| Current compiler | 2600 | 2600 |
| Step 1: Inlining | 50 | 2650 |
| Steps 4-8: Interpreter | 325 | 2975 |

**Final: ~3000 lines of Forth.**

For comparison:
- GCC: 15,000,000 lines
- LLVM: 20,000,000 lines
- Sixth: 3,000 lines

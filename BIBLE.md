# Sixth: The Plan

One tool. Written in Forth. Compiles itself. No dependencies.

---

## What We Are Building

A native x86-64 compiler written in Forth that:

1. Beats GCC -O2 on runtime performance
2. Interprets for development
3. Compiles for deployment
4. Self-hosts (compiles itself)
5. No C. No external dependencies.

---

## Current State

| Component | Lines | Status |
|-----------|-------|--------|
| Compiler core | ~2600 | Working |
| Constant folding | ✓ | Done |
| Literal-op fusion | ✓ | Done |
| Register stack | ✓ | Done |
| Tail call optimization | ✓ | Done |
| Loop elimination | ✓ | Done |
| Swap absorption | ✓ | Done |
| Dup+cmp fusion | ✓ | Done |

**Missing for GCC parity:**

| Feature | Lines | Purpose |
|---------|-------|---------|
| Inlining | ~50 | Copy small words inline instead of call |
| Strength reduction | ~30 | `8 *` → `3 lshift` |

**Missing for self-hosting:**

| Feature | Lines | Purpose |
|---------|-------|---------|
| Interpreter words | ~325 | Kill the C interpreter |

---

## Phase 1: Beat GCC -O2 (80 lines)

### 1.1 Inlining (~50 lines)

Words under 20 bytes: copy code inline instead of `call`.

```forth
: 2*  2* ;   \ 3 bytes — inline it
: big-word  ... 200 bytes ... ;   \ call it
```

**Implementation:**

1. Track code size of each word in dictionary
2. At call site, check size
3. If ≤20 bytes: emit the bytes directly
4. If >20 bytes: emit `call`

**Why this beats GCC:**

GCC cannot inline across compilation units. You inline everything. Every small helper becomes zero-cost.

### 1.2 Strength Reduction (~30 lines)

Pattern match on literals. Emit faster instructions.

```
8 *    →  3 lshift      (3 bytes vs 7)
4 /    →  2 rshift      (unsigned only!)
8 mod  →  7 and         (power of 2 only)
```

**WARNING:** Do NOT optimize signed `/`. Shift rounds toward -∞, idiv rounds toward 0. Different for negatives.

**Implementation:**

1. In `compile-builtin` for `*`, check if literal is power of 2
2. If yes, emit `shl` with log2
3. Same for unsigned `/` and `mod`

---

## Phase 2: Self-Hosting Interpreter (325 lines)

Kill the C interpreter. One tool does everything.

### 2.1 Input Parsing (~100 lines)

| Word | Stack | Purpose |
|------|-------|---------|
| `SOURCE` | `( -- addr u )` | Current input buffer |
| `>IN` | `( -- addr )` | Parse position variable |
| `WORD` | `( char -- c-addr )` | Parse delimited token |
| `PARSE` | `( char -- addr u )` | Parse to delimiter |
| `EVALUATE` | `( addr u -- )` | Interpret string |

### 2.2 Dictionary Lookup (~60 lines)

| Word | Stack | Purpose |
|------|-------|---------|
| `FIND` | `( c-addr -- c-addr 0 \| xt 1 \| xt -1 )` | Look up word |
| `'` | `( "name" -- xt )` | Parse and find |
| `[']` | Compile-time `'` | Compile xt as literal |
| `EXECUTE` | `( xt -- )` | Run xt |
| `>BODY` | `( xt -- addr )` | Data field of CREATE'd word |

### 2.3 Defining Words (~80 lines)

| Word | Purpose |
|------|---------|
| `CREATE` | Make dictionary entry, push data address at runtime |
| `DOES>` | Attach runtime behavior to CREATE'd word |

This enables user-defined defining words:

```forth
: constant  create , does> @ ;
: variable  create 0 , ;
```

### 2.4 Compilation State (~20 lines)

| Word | Stack | Purpose |
|------|-------|---------|
| `STATE` | `( -- addr )` | 0=interpret, nonzero=compile |
| `[` | `( -- )` | Switch to interpret mode |
| `]` | `( -- )` | Switch to compile mode |

### 2.5 Miscellaneous (~65 lines)

| Word | Lines | Purpose |
|------|-------|---------|
| `ACCEPT` | ~30 | Read line from user |
| `ALLOT` | ~5 | Already exists |
| `ALIGN ALIGNED` | ~15 | Memory alignment |
| `UNLOOP` | ~10 | Clean return stack on LEAVE |
| `HEX DECIMAL` | ~5 | Set base |

---

## Phase 3: Delete C (0 lines)

Once Phase 2 is complete:

1. `./sixth sixth.fs -o sixth` — compile the compiler
2. `./sixth` — runs natively, interprets and compiles
3. `rm -rf engine/` — delete the C interpreter

No more C compiler dependency. No more GCC. No more Clang.

Sixth compiles Sixth.

---

## Line Count

| Phase | Lines | Running Total |
|-------|-------|---------------|
| Current compiler | 2600 | 2600 |
| Phase 1: Beat GCC | 80 | 2680 |
| Phase 2: Self-host | 325 | 3005 |

**Final: ~3000 lines of Forth.**

For comparison:
- GCC: 15,000,000 lines
- LLVM: 20,000,000 lines
- Sixth: 3,000 lines

---

## Benchmarks

Primitive isolation tests in `compiler/bench/`:

| Test | Stresses |
|------|----------|
| arith.fs / arith.c | Pure arithmetic |
| mem.fs / mem.c | Pure memory |
| ctrl.fs / ctrl.c | Pure control flow |
| call.fs / call.c | Call overhead |
| shift.fs / shift.c | Shift operations |
| double.fs / double.c | Double-cell arithmetic |

Mixed workloads:

| Test | Stresses |
|------|----------|
| fib40 | Recursion |
| ack | Deep recursion |
| tak | Heavy recursion |
| collatz | Conditionals |
| primes | Division |
| sieve1m | Byte memory |
| matmul | Strided access |

**Method:**

```bash
gcc -O2 bench.c -o bench_c
./sixth bench.fs -o bench_fs
time ./bench_c
time ./bench_fs
```

Loser reveals weakness. Fix. Repeat.

---

## Order of Implementation

1. **Inlining** — biggest performance win
2. **Strength reduction** — easy, high impact
3. **Run benchmarks** — verify GCC parity
4. **Input parsing** — foundation for interpreter
5. **Dictionary lookup** — FIND, EXECUTE
6. **CREATE DOES>** — defining words
7. **STATE [ ]** — mode switching
8. **ACCEPT** — interactive input
9. **Delete C** — freedom

---

## Success Criteria

Phase 1 complete when:
- All benchmarks match or beat GCC -O2

Phase 2 complete when:
- `./sixth sixth.fs -o sixth` produces working binary
- `./sixth` can interpret and compile interactively
- Hayes ANS Forth test suite passes

Phase 3 complete when:
- `engine/` directory deleted
- No C compiler required to build or run Sixth

---

## The Point

3000 lines of Forth replaces 15 million lines of C.

You understand every line. You can fix any bug. You depend on nothing.

That is the point.

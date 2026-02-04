# Sixth Compiler Refactoring Plan

## Signal Analysis

sixth.fs is 3,487 lines with approximately 5 structural sections that share a flat variable scope. The persistent debugging difficulty traces to one root cause: **state mutations are distributed, implicit, and unverifiable**. Specifically, `stack-depth` is adjusted manually in 100+ locations, three pending-state variables interact through 330 lines of dispatch code, and identical patterns are copied rather than shared, making it impossible to distinguish correct instances from incorrect ones by inspection.

The five refactoring phases below are ordered by debugging leverage: each phase makes the next phase's bugs easier to find.

---

## Phase 1: Stack-Depth Verification Layer

### Problem

Every `gen-*` function manually adjusts `stack-depth` via `+!`. There are ~60 such adjustments across codegen. A single wrong number produces silent corruption: registers are saved/restored incorrectly, and the bug manifests far from its source as a wrong value or segfault.

The three pending-state variables (`swap-pending`, `dup-pending`, `cmp-pending`) compound the problem. Their flush functions (`flush-swap`, `flush-cmp`) also modify `stack-depth` indirectly through `gen-dup` and `gen-swap`. When a `gen-*` function calls `pop-nos` or `pop-tos`, those also modify `stack-depth`. The true stack effect of any given codegen path requires tracing through 2-3 levels of calls.

### Concrete locations

| Function | Line | Adjustment | Declared effect |
|----------|------|------------|-----------------|
| `push-tos` | 281 | `1 stack-depth +!` | ( -- x ) |
| `pop-tos` | 293 | `-1 stack-depth +!` | ( x -- ) |
| `pop-nos` | 304 | `-1 stack-depth +!` | ( x y -- x ) |
| `gen-move` | 512 | `-3 stack-depth +!` | ( src dst u -- ) |
| `gen-fill` | 540 | `-3 stack-depth +!` | ( addr u char -- ) |
| `gen-!` | 990 | `-2 stack-depth +!` | ( val addr -- ) |
| `gen-c!` | 1004 | `-2 stack-depth +!` | ( char addr -- ) |
| `gen-+!` | 1016 | `-2 stack-depth +!` | ( n addr -- ) |
| `gen->r` | 1033 | conditional `-1` | ( x -- R: x ) |
| `gen-type` | 1609 | `-2 stack-depth +!` + conditional promote | ( addr u -- ) |
| `gen-call` | 1282 | `call-rets - call-nargs` | varies |
| `gen-within` | 791 | `-2 stack-depth +!` | ( u lo hi -- flag ) |
| `gen-*/mod` | 602 | `-1 stack-depth +!` | ( n1 n2 n3 -- rem quot ) |
| `gen-*/` | 618 | `-1 stack-depth +!` + `pop-nos` | ( n1 n2 n3 -- quot ) |
| `gen-d+` | 955 | `-2 stack-depth +!` | ( d1lo d1hi d2lo d2hi -- dlo dhi ) |
| `gen-d-` | 972 | `-2 stack-depth +!` | ( d1lo d1hi d2lo d2hi -- dlo dhi ) |
| `gen-um/mod` | 889 | `-1 stack-depth +!` | ( udlo udhi u1 -- ur uq ) |
| `gen-sm/rem` | 905 | `-1 stack-depth +!` | ( dlo dhi n -- rem quot ) |
| `gen-fm/mod` | 933 | `-1 stack-depth +!` | ( dlo dhi n -- rem quot ) |
| `gen-open-file` | 1712 | `-1 stack-depth +!` | ( addr u fam -- fid ior ) |
| `gen-create-file` | 1744 | `-1 stack-depth +!` | ( addr u fam -- fid ior ) |
| `gen-read-file` | 1783 | `-1 stack-depth +!` | ( addr u fid -- u2 ior ) |
| `gen-write-file` | 1807 | `-2 stack-depth +!` | ( addr u fid -- ior ) |
| `gen-argv` | 1674 | `1 stack-depth +!` | ( n -- addr u ) |
| `gen-argc` | 1656 | `1 stack-depth +!` | ( -- n ) |
| `gen-accept` | 2020 | `-1 stack-depth +!` | ( addr u1 -- u2 ) |
| `gen-parse` | 1980 | `1 stack-depth +!` | ( char -- addr u ) |
| `gen-word` | 1995 | `-1 stack-depth +!` | ( char -- c-addr ) |
| `gen-evaluate` | 2187 | `-2 stack-depth +!` | ( addr u -- ) |
| `gen-execute` | 2081 | `-1 stack-depth +!` | ( xt -- ) |
| `gen-lshift` | 828 | `-1 stack-depth +!` | ( val count -- result ) |
| `gen-rshift` | 845 | `-1 stack-depth +!` | ( val count -- result ) |
| `gen-spaces` | 467 | `pop-tos` | ( n -- ) |

### Implementation

1. Define a verification word that wraps stack-effect checking:

```forth
: assert-depth ( expected-depth label-addr label-u -- )
  stack-depth @ rot <> if
    ." DEPTH MISMATCH in " type
    ."  expected=" . ."  got=" stack-depth @ . cr
    1 throw
  else 2drop then ;
```

2. At every control-flow join point in `compile-builtin`, assert that `stack-depth` matches:

   - `$then`: the depth after the `if` branch must equal the depth restored from `cf-pop`. Already stored (line 2843: `cf-pop stack-depth !`), but no check that the current depth matches.
   - `$repeat` / `$again`: depth at back-edge must equal depth at `$begin`.
   - `$loop` / `$+loop`: depth must equal what it was at `$do`.

3. Add compile-time checks in `end-def` that `stack-depth` equals the declared return count.

4. Instrument `gen-call` — the most complex adjustment (line 1282: `call-rets @ call-nargs @ - stack-depth +!`). Log when `call-nargs` or `call-rets` differs from what info-buf says.

### Regression test

```bash
# Gate: all existing tests must pass with assertions enabled
./compiler/tests/test
# Expected: TOTAL: 44  PASS: 44  WRONG: 0  CFAIL: 0  RFAIL: 0

# If any assertion fires, it identifies the exact gen-* function and
# the exact word being compiled where stack-depth diverged.
```

If assertions fire on existing passing tests, those are latent bugs that happen to not manifest in the test output. Fix them before proceeding.

---

## Phase 2: Dispatch Table Compression

### Problem

`compile-builtin` (lines 2581-2911) is a 330-line linear `if/else` chain. Each entry does:

1. `2dup $name str=` — string compare
2. `2drop` — consume the token
3. Some combination of `flush-swap`, `ct-flush`, `flush-pending`
4. Call a `gen-*` function
5. `true exit` — return found

The flush combination follows one of exactly 5 patterns:

| Pattern | Meaning | Count |
|---------|---------|-------|
| `flush-swap ct-flush` | stack op, no pending | ~15 |
| `flush-swap ct-flush flush-pending` | consumes args | ~40 |
| `flush-swap ct-flush gen-X` + fold logic | arithmetic | ~15 |
| pending-state manipulation | dup/swap/cmp | ~8 |
| control flow | if/then/begin/while/do | ~15 |

The first two patterns (~55 entries) are pure data. They carry zero information beyond "this name maps to this gen-* function with this flush mode."

### Implementation

1. Define a dispatch table structure:

```forth
\ Entry: name-2const(2 cells) + gen-xt(1 cell) + flush-mode(1 cell) = 4 cells
\ flush-mode: 0=flush-swap+ct-flush, 1=flush-swap+ct-flush+flush-pending
create builtin-table 128 4 * cells allot
variable builtin-count  0 builtin-count !
```

2. Define a registration word:

```forth
: register-builtin ( name-addr name-u gen-xt flush-mode -- )
  builtin-count @ 4 * cells builtin-table +
  >r r@ 3 cells + !  r@ 2 cells + !  r@ cell+ !  r@ !
  r> drop  1 builtin-count +! ;
```

3. Table entries for the simple cases:

```forth
$cr    ' gen-cr    0 register-builtin
$.     ' gen-dot   1 register-builtin
$emit  ' gen-emit  1 register-builtin
\ ... ~55 entries
```

4. Dispatcher:

```forth
: dispatch-builtin ( addr u -- found? )
  builtin-count @ 0 ?do
    i 4 * cells builtin-table + >r
    2dup r@ @ r@ cell+ @ str= if
      2drop
      r@ 3 cells + @ ?dup if  \ flush-mode
        flush-swap ct-flush flush-pending
      else
        flush-swap ct-flush
      then
      r@ 2 cells + @ execute
      r> drop true unloop exit
    then
    r> drop
  loop
  false ;
```

5. `compile-builtin` shrinks to: call `dispatch-builtin`; if false, fall through to the ~25 special cases (fold/fuse arithmetic, pending-state, control flow) which remain as hand-written code.

### What stays hand-written

These cases have logic beyond "flush + call gen-*":

- `dup`, `swap`, `drop` — pending-state manipulation
- `+`, `-`, `*`, `mod`, `and`, `or`, `xor` — ct-depth fold/fuse
- `negate`, `1+`, `1-`, `2*`, `2/`, `invert`, `abs` — unary fold
- `0=`, `0<`, `0>` — cmp-pending
- `if`, `else`, `then`, `begin`, `while`, `until`, `repeat`, `again` — control flow
- `do`, `?do`, `loop`, `+loop` — loop control
- `s"`, `."` — string parsing

Total: ~25-30 cases with real logic, down from 130+ entries.

### Regression test

```bash
# Same gate:
./compiler/tests/test
# Must produce identical results to pre-refactor.

# Additional: compile a non-trivial program (the test suite's adversarial tests)
# and binary-diff the output ELF. The generated code must be byte-identical.
for f in compiler/tests/adversarial/*.fs; do
  ./engine/fifth compiler/sixth.fs "$f" /tmp/before-refactor 2>/dev/null
  ./engine/fifth compiler/sixth-refactored.fs "$f" /tmp/after-refactor 2>/dev/null
  if ! cmp -s /tmp/before-refactor /tmp/after-refactor; then
    echo "MISMATCH: $f"
  fi
done
```

Byte-identical output proves the refactoring changed nothing.

---

## Phase 3: Deduplicate Stack-Comment Parsing

### Problem

Two functions do the same job:

| Function | Lines | Writes to | Called by |
|----------|-------|-----------|----------|
| `scan-stack-comment` | 2956-2987 | `scan-nargs`, `scan-rets`, `scan-void` | `scan-all` (pass 1) |
| `parse-stack-comment` | 3110-3145 | `arg-count`, `ret-count`, `is-void` | `start-def` (pass 2) |

They've already diverged: `scan-stack-comment` uses `scan-skip-ws` while `parse-stack-comment` uses `skip-ws-only`. Both are also copies of each other (lines 2948-2954 vs 3102-3108). The `scan-` version checks `dup 32 <= swap 0 > and` (skip chars 1-32), the `parse-` version checks identically. Today they match. Tomorrow someone fixes one.

### Implementation

1. Delete `scan-stack-comment`, `scan-skip-ws`, `parse-stack-comment`, `skip-ws-only`.

2. Replace with one function:

```forth
: parse-stack-effect ( -- nargs nrets is-void )
  \ Skip whitespace
  begin
    input-pos @ input-len @ >= if 0 1 1 exit then
    input-buf input-pos @ + c@ dup 32 <= swap 0 > and
  while 1 input-pos +! repeat

  \ Check for ( opening
  input-pos @ input-len @ >= if 0 1 1 exit then
  input-buf input-pos @ + c@ [char] ( <> if 0 1 1 exit then
  input-pos @ 1+ input-len @ >= if 0 1 1 exit then
  input-buf input-pos @ 1+ + c@ 32 > if 0 1 1 exit then
  1 input-pos +!

  \ Parse: count items before --, count items after --
  0 0 1   ( nargs nrets is-void )
  0 >r    ( R: past-separator )
  begin
    \ skip whitespace inside parens
    begin
      input-pos @ input-len @ >= if r> drop exit then
      input-buf input-pos @ + c@ dup 32 <= swap 0 > and
    while 1 input-pos +! repeat

    input-pos @ input-len @ >= if r> drop exit then
    input-buf input-pos @ + c@
    dup [char] ) = if drop r> drop exit then
    dup [char] - = if
      input-pos @ 1+ input-len @ < if
        input-buf input-pos @ 1+ + c@ [char] - = if
          drop 2 input-pos +! r> drop 1 >r
        else
          drop
          begin input-pos @ input-len @ < while
            input-buf input-pos @ + c@ 32 > while
            1 input-pos +!
          repeat then
          r@ if rot drop 0 -rot 1+ then
          r@ 0= if rot 1+ -rot then
        then
      else
        drop
        begin input-pos @ input-len @ < while
          input-buf input-pos @ + c@ 32 > while
          1 input-pos +!
        repeat then
        r@ if rot drop 0 -rot 1+ then
        r@ 0= if rot 1+ -rot then
      then
    else
      drop
      begin input-pos @ input-len @ < while
        input-buf input-pos @ + c@ 32 > while
        1 input-pos +!
      repeat then
      r@ if rot drop 0 -rot 1+ then
      r@ 0= if rot 1+ -rot then
    then
  again ;
```

3. Callers:

```forth
\ In scan-all:
parse-stack-effect scan-void ! scan-rets ! scan-nargs !

\ In start-def:
parse-stack-effect is-void ! ret-count ! arg-count !
```

### Regression test

```bash
./compiler/tests/test
# Gate: identical results.

# Targeted: test edge cases in stack comments
# Create temp test files with various stack comment formats:
# ( -- )        → nargs=0 nrets=0 void=1
# ( n -- )      → nargs=1 nrets=0 void=1
# ( n -- m )    → nargs=1 nrets=1 void=0
# ( a b -- c d ) → nargs=2 nrets=2 void=0
# ( -- n )      → nargs=0 nrets=1 void=0
# No comment    → defaults (nargs=0 nrets=1 void=1 — verify against current behavior)
```

Write a small test for each case: define a word with that stack comment, call it, verify correct register save/restore by checking output.

---

## Phase 4: Factor Redundant Codegen

### Problem

Identical or near-identical code is repeated in multiple `gen-*` functions. Each copy is an independent bug surface.

### Group A: Comparison operators (7 functions, lines 710-773)

All seven follow this pattern:
```forth
: gen-XX ( -- )
  gen-cmp-setup
  $0f c, $CC c, $c0 c,           \ setCC al
  $48 c, $0f c, $b6 c, $c0 c,   \ movzx rax, al
  $48 c, $f7 c, $d8 c, ;         \ neg rax
```

Where `$CC` is: `$94` (=), `$95` (<>), `$9c` (<), `$9f` (>), `$9e` (<=), `$9d` (>=), `$92` (u<).

**Replacement:**
```forth
: gen-setcc ( cc-byte -- )
  gen-cmp-setup
  $0f c, c, $c0 c,
  $48 c, $0f c, $b6 c, $c0 c,
  $48 c, $f7 c, $d8 c, ;

: gen-= ( -- )   $94 gen-setcc ;
: gen-<> ( -- )  $95 gen-setcc ;
: gen-< ( -- )   $9c gen-setcc ;
: gen-> ( -- )   $9f gen-setcc ;
: gen-<= ( -- )  $9e gen-setcc ;
: gen->= ( -- )  $9d gen-setcc ;
: gen-u< ( -- )  $92 gen-setcc ;
```

77 lines become 18.

### Group B: 3-arg stack promotion (gen-move, gen-fill, lines 486-540)

Both functions load arg from 3rd position, do their operation, then run identical register promotion code (~20 lines). Factor into:

```forth
: gen-3arg-setup ( -- )  \ load 3rd arg into appropriate register
  stack-depth @ 3 >= if ... else ... then ;

: gen-3arg-promote ( -- )  \ promote remaining items
  stack-depth @ 4 >= if ... then
  -3 stack-depth +! ;
```

### Group C: Syscall register save/restore (gen-dot, gen-cr, gen-emit, gen-type, gen-space, gen-key)

All I/O functions follow:
```
save rbx if depth>=2, save rcx if depth>=3
... do syscall ...
restore rcx if depth>=3, restore rbx if depth>=2
```

Factor into:
```forth
: save-regs ( -- )
  stack-depth @ 2 >= if $53 c, then
  stack-depth @ 3 >= if $51 c, then ;

: restore-regs ( -- )
  stack-depth @ 3 >= if $59 c, then
  stack-depth @ 2 >= if $5b c, then ;
```

### Group D: File I/O (gen-open-file, gen-create-file, lines 1676-1745)

69 lines, ~90% identical. The only difference: `gen-create-file` adds `or rax, O_CREAT|O_TRUNC` before the syscall. Factor into `gen-open-file-core ( add-flags -- )`.

### Group E: 3-arg division setup (gen-um/mod, gen-sm/rem, gen-fm/mod)

Same register shuffle, differ only in `div` vs `idiv` and post-fixup. Factor the setup into shared code, parameterize the division instruction.

### Implementation order

A first (largest line reduction, simplest), then B and C (medium), then D and E.

### Regression test

```bash
./compiler/tests/test
# Gate: identical results.

# Binary comparison: for every test, the compiled output must be
# byte-identical before and after factoring.
for f in compiler/tests/*.fs; do
  ./engine/fifth compiler/sixth.fs "$f" /tmp/before 2>/dev/null
  ./engine/fifth compiler/sixth-factored.fs "$f" /tmp/after 2>/dev/null
  if ! cmp -s /tmp/before /tmp/after; then
    echo "BINARY DIFF: $f"
  fi
done
```

Byte-identical output is mandatory. These are pure refactors — no behavior change.

---

## Phase 5: File Split

### Problem

All 3,487 lines share a flat scope. Any variable is accessible from anywhere. The lack of module boundaries means:
- You can't test codegen without the tokenizer.
- You can't test the tokenizer without the codegen.
- A variable name collision silently overwrites.

### Proposed split

```
compiler/
  sixth.fs           → driver (load order, main entry)
  sixth-buffers.fs   → buffer declarations, state variables (lines 1-130)
  sixth-codegen.fs   → all gen-* functions (lines 270-2210)
  sixth-parse.fs     → tokenizer, number parser (lines 2430-2560)
  sixth-dispatch.fs  → compile-builtin table + special cases (lines 2560-2911)
  sixth-compiler.fs  → compile-token, compile-word, compile-all, scan-all (lines 2912-3457)
```

### Interface boundaries

| File | Reads | Writes |
|------|-------|--------|
| `sixth-buffers.fs` | nothing | declares all shared state |
| `sixth-codegen.fs` | `stack-depth`, `code-pos`, `code-buf`, `has-io` | `code-buf` (via `c,`), `stack-depth` |
| `sixth-parse.fs` | `input-buf`, `input-pos`, `input-len` | `input-pos`, `token-buf`, `token-len` |
| `sixth-dispatch.fs` | `ct-depth`, `ct-stack`, pending vars | pending vars, calls `gen-*` |
| `sixth-compiler.fs` | everything | everything |

### Implementation

1. Start by extracting `sixth-buffers.fs` — zero risk, just move declarations.
2. Extract `sixth-codegen.fs` — move all `gen-*` words. Verify they only depend on buffers.
3. Extract `sixth-parse.fs` — move tokenizer and number parser.
4. Extract `sixth-dispatch.fs` — move `compile-builtin` and string constants.
5. `sixth.fs` becomes the include driver + `compile-file` + `main-entry`.

### Load order in new `sixth.fs`

```forth
include compiler/sixth-buffers.fs
include compiler/sixth-codegen.fs
include compiler/sixth-parse.fs
include compiler/sixth-dispatch.fs
include compiler/sixth-compiler.fs
```

### Regression test

```bash
./compiler/tests/test
# Gate: identical results.

# Binary comparison: same protocol as Phase 4.
# The split must produce byte-identical compiled output.
```

---

## Sequential Protocol

### Invariant

After every phase, before proceeding to the next:

```bash
./compiler/tests/test
```

Must produce:
```
TOTAL: 44  PASS: 44  WRONG: 0  CFAIL: 0  RFAIL: 0
```

Any regression blocks the current phase until fixed. Do not carry forward failures.

### Execution order

```
Phase 1: Stack-depth verification
  │
  ├─ Add assert-depth word
  ├─ Instrument control-flow join points (then, repeat, loop)
  ├─ Instrument end-def (verify final depth = declared rets)
  ├─ Instrument gen-call (log nargs/nrets mismatches)
  ├─ RUN TESTS
  ├─ Fix any assertions that fire (these are the bugs)
  ├─ RUN TESTS (must be clean)
  │
  ▼
Phase 2: Dispatch table
  │
  ├─ Define table structure and registration word
  ├─ Move ~55 simple entries to table
  ├─ Write dispatcher, wire into compile-builtin
  ├─ RUN TESTS
  ├─ Binary-diff compiled outputs (must be identical)
  ├─ Delete dead code from old compile-builtin
  ├─ RUN TESTS
  │
  ▼
Phase 3: Deduplicate stack-comment parsing
  │
  ├─ Write unified parse-stack-effect
  ├─ Replace scan-stack-comment calls
  ├─ Replace parse-stack-comment calls
  ├─ Delete old functions + old skip-ws variants
  ├─ RUN TESTS
  │
  ▼
Phase 4: Factor codegen
  │
  ├─ Group A: gen-setcc (7 comparison ops)
  ├─ RUN TESTS + binary-diff
  ├─ Group B: gen-3arg-setup/promote (move, fill)
  ├─ RUN TESTS + binary-diff
  ├─ Group C: save-regs/restore-regs (I/O ops)
  ├─ RUN TESTS + binary-diff
  ├─ Group D: gen-open-file-core (file I/O)
  ├─ RUN TESTS + binary-diff
  ├─ Group E: division setup (um/mod, sm/rem, fm/mod)
  ├─ RUN TESTS + binary-diff
  │
  ▼
Phase 5: File split
  │
  ├─ Extract sixth-buffers.fs
  ├─ RUN TESTS
  ├─ Extract sixth-codegen.fs
  ├─ RUN TESTS
  ├─ Extract sixth-parse.fs
  ├─ RUN TESTS
  ├─ Extract sixth-dispatch.fs
  ├─ RUN TESTS
  ├─ Verify final sixth.fs is driver-only
  ├─ RUN TESTS + binary-diff full suite
  │
  ▼
  DONE
```

### Binary-diff script

Save this as `compiler/tests/binary-diff`:

```bash
#!/bin/bash
# Compare compiled output between two compiler versions.
# Usage: binary-diff <compiler-a> <compiler-b>
A="$1"
B="$2"
PASS=0
FAIL=0
for f in compiler/tests/*.fs; do
  ./engine/fifth "$A" "$f" /tmp/bda 2>/dev/null
  SA=$?
  ./engine/fifth "$B" "$f" /tmp/bdb 2>/dev/null
  SB=$?
  if [ "$SA" != "$SB" ]; then
    echo "EXIT DIFF: $f (a=$SA b=$SB)"
    FAIL=$((FAIL+1))
    continue
  fi
  if [ "$SA" != "0" ]; then
    PASS=$((PASS+1))  # both failed to compile, that's consistent
    continue
  fi
  if cmp -s /tmp/bda /tmp/bdb; then
    PASS=$((PASS+1))
  else
    echo "BINARY DIFF: $f"
    FAIL=$((FAIL+1))
  fi
done
echo "PASS=$PASS FAIL=$FAIL"
[ "$FAIL" = "0" ]
```

### Time budget

Phase 1 is the most important. If it surfaces the bug, the remaining phases become cleanup rather than debugging. Phases 2-5 reduce the surface area for future bugs.

### What to do when Phase 1 assertions fire

1. The assertion message identifies the word being compiled and the expected vs actual depth.
2. Trace backward: which `gen-*` call produced the wrong depth?
3. Check that `gen-*` function's stack-depth adjustment against the table in Phase 1.
4. Fix the adjustment.
5. Re-run tests. If the fix changes test output (a "passing" test was actually wrong), examine the test's expected output.

This is the protocol. The bug will surface in Phase 1.

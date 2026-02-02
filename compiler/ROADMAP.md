# Sixth Native Compiler Roadmap

## The Two Laws

**1. Sixth depends on nothing.**

No bash. No shell scripts. No piping. No heredocs. No shelling out. Every external dependency is a failure. Every line of bash is an admission that Forth is incomplete.

When complete, Sixth needs only a kernel that loads ELF binaries. Everything else is Forth.

**2. Sixth outperforms GCC -O2.**

Not "acceptable performance." Not "close enough." Faster. On the benchmarks that matter — recursive functions, tight loops, integer math — Sixth-compiled code beats GCC -O2 compiled C.

This is possible because:
- Stack caching keeps hot values in registers (no memory traffic)
- Superinstructions fuse common patterns into single ops
- Constant folding eliminates work at compile time
- No function call overhead for small words (inlined)
- Single-pass compilation means no IR bloat

The goal is not a small line count — the goal is sovereignty AND speed.

---

## Current State

~2800 lines of Forth. Compiles to x86-64 machine code. Single-pass.

### Compiled Words (115+ total)

**Arithmetic**: `+ - * / mod /mod */mod */ negate abs 1+ 1- 2+ 2- 2* 2/`
**Comparison**: `= <> < > 0= 0< 0> 0<> <= >= u< within min max`
**Logic**: `and or xor invert`
**Stack**: `dup drop swap over rot nip tuck 2dup 2drop 2swap 2over ?dup depth pick`
**Control**: `if else then begin until while repeat again do loop +loop i j leave recurse recursive exit`
**Memory**: `@ ! c@ c! +! cells cell+ chars char+ variable constant create allot here , c, move fill`
**Return Stack**: `>r r> r@ 2>r 2r> 2r@`
**I/O**: `. u. emit cr type space spaces key ." s"`
**Parsing**: `source >in parse word accept refill count`
**Shifts**: `lshift rshift`
**Double**: `s>d um* m* um/mod sm/rem fm/mod d+ d-`
**Pictured**: `<# # #s #> hold sign base`
**Fused**: `nos+ tuck+ dup2 <if >if =if 0<if 0=if 1-nzloop nzloop 0=until`

### Testing

```
./sixth compiler/tests/run.fs
```

1600 pass, 0 wrong, 51 skip. Single command. No bash.

---

## Linear Checklist

**What remains, in order:**

```
[x] 1.  '        ( "name" -- xt )         parse and find
[x] 2.  [']      ( "name" -- )            compile xt as literal (use: ' word)
[x] 3.  >BODY    ( xt -- addr )           get data field
[x] 4.  [        ( -- )                   switch to interpret mode (immediate)
[x] 5.  ]        ( -- )                   switch to compile mode
[x] 6.  LITERAL  ( n -- )                 compile literal (immediate)
[x] 7.  ABORT    ( -- )                   clear stacks, restart
[x] 8.  QUIT     ( -- )                   main REPL loop (words only, no number parsing)
[ ] 9.  POSTPONE ( "name" -- )            compile compilation (immediate)
[ ] 10. DOES>    ( -- )                   set runtime behavior
[x] 11. OPEN-FILE                         syscall wrapper
[x] 12. READ-FILE                         syscall wrapper
[ ] 13. READ-LINE                         line-by-line reading
[ ] 14. INCLUDE  ( "name" -- )            load and evaluate file
------- SELF-HOSTING COMPLETE -------
[ ] 15. Delete engine/                    remove C interpreter
[ ] 16. Forth test framework              replace bash test runner
------- SOVEREIGNTY COMPLETE -------
[ ] 17. CLOCK-MS                          syscall 228 (timing)
[ ] 18. ARGC ARGV                         command line args
[ ] 19. GETENV                            environment variables
[ ] 20. BYE                               syscall 60 (clean exit)
[ ] 21. THROW CATCH                       exception handling
------- PRACTICAL COMPLETE -------
```

### Missing Syscall Wrappers

Words in interpreter but not native compiler. Required for practical use.

| Word | Syscall | Purpose | Lines |
|------|---------|---------|-------|
| `clock-ms` | 228 (clock_gettime) | Timing, benchmarks | ~15 |
| `argc` `argv` | N/A (kernel passes at startup) | Command line args | ~20 |
| `getenv` | N/A (parse environ pointer) | Environment variables | ~25 |
| `bye` | 60 (exit) | Clean exit from REPL | ~5 |
| `throw` `catch` | N/A (stack manipulation) | Exception handling | ~40 |

**Notes:**
- `argc`/`argv`: Linux puts these on stack at `[rsp]` and `[rsp+8]` at program start
- `getenv`: `environ` pointer follows `argv` array (after NULL terminator)
- `throw`/`catch`: Pure Forth, no syscall. Save/restore return stack.

**Already done:**
- [x] SOURCE, >IN, PARSE, WORD, ACCEPT, REFILL (Phase 1)
- [x] FIND, EXECUTE (runtime dictionary lookup)
- [x] INTERPRET, EVALUATE (interpreter loop core)
- [x] STATE, CREATE, : ; IMMEDIATE (defining words)
- [x] ', >BODY, [, ], LITERAL (Phase 2-3)
- [x] OPEN-FILE, READ-FILE, WRITE-FILE, CLOSE-FILE, r/o, w/o, r/w (Phase 6)
- [x] QUIT, ABORT (Phase 5 - REPL loop, words only)

---

## Self-Hosting Roadmap

Goal: Full interactive Forth system. Interprets, compiles, no C.

### Phase 1: Input Parsing — DONE

### Phase 2: Dictionary — MOSTLY DONE

| Word | Status |
|------|--------|
| FIND | DONE |
| EXECUTE | DONE |
| ' | NOT STARTED |
| ['] | NOT STARTED |
| >BODY | NOT STARTED |

### Phase 3: State Machine — NOT STARTED

| Word | Status |
|------|--------|
| STATE | DONE |
| [ | NOT STARTED |
| ] | NOT STARTED |
| LITERAL | NOT STARTED |

### Phase 4: Defining Words — DONE

| Word | Status |
|------|--------|
| CREATE | DONE |
| : ; | DONE |
| IMMEDIATE | DONE |

### Phase 5: Interpreter Loop — PARTIAL

| Word | Status |
|------|--------|
| INTERPRET | DONE |
| EVALUATE | DONE |
| ABORT | NOT STARTED |
| QUIT | NOT STARTED |
| POSTPONE | NOT STARTED |
| DOES> | NOT STARTED |

#### Implementation Order

Build in this exact order. Each word depends on the ones before it.

**1. INTERPRET — DONE**

**2. EVALUATE — DONE**

**3. ABORT — next**
- Simple: clear stacks, call QUIT
- Error recovery

**4. QUIT — after ABORT**
- Main REPL loop
- Calls REFILL, INTERPRET, repeats

**5. POSTPONE**
- Compiles code that compiles
- Used for defining control structures in Forth

**6. DOES>**
- Sets runtime behavior of CREATE'd words
- Used to define CONSTANT, VARIABLE, etc. in Forth itself

#### Dependency Graph

```
INTERPRET
    ↓
EVALUATE ← needed by INCLUDE, [ ... ]
    ↓
POSTPONE ← optional but clean
    ↓
DOES>

ABORT → QUIT → INTERPRET (circular, but QUIT is just the loop)

OPEN-FILE → READ-FILE → READ-LINE → INCLUDE → EVALUATE
```

### Phase 6: File I/O — PARTIAL

| Word | Status | Est. Lines |
|------|--------|------------|
| slurp-file | DONE (in interpreter) | — |
| write-file | DONE (for ELF output) | — |
| close-file | DONE | — |
| OPEN-FILE | NOT STARTED | ~15 |
| READ-FILE | NOT STARTED | ~15 |
| READ-LINE | NOT STARTED | ~20 |
| INCLUDE | NOT STARTED | ~20 |

~70 lines total.

#### Implementation Order

**7. OPEN-FILE**
- Syscall wrapper (open)
- Returns file handle

**8. READ-FILE**
- Syscall wrapper (read)
- Needs file handle from OPEN-FILE

**9. READ-LINE**
- Uses READ-FILE
- Scans for newline
- Used by INCLUDE for line-by-line loading

**10. INCLUDE — last**
- Opens file (OPEN-FILE)
- Reads lines (READ-LINE)
- Evaluates each line (EVALUATE)
- Closes file

This completes the interpreter. Hayes tests can run.

### Phase 7: Cleanup — NOT STARTED

| Task | Purpose |
|------|---------|
| Delete engine/ | Remove C interpreter — **eliminates GCC** |

### Phase 8: Native SQLite — NOT STARTED

| Task | Est. Lines | Eliminates |
|------|------------|------------|
| SQLite C API bindings | ~100 | `sqlite3` shell-out |

Direct syscalls to SQLite shared library. No more `system("sqlite3 ...")`.

### Phase 9: Test Sovereignty — NOT STARTED

| Task | Est. Lines | Eliminates |
|------|------------|------------|
| Test framework in Forth | ~50 | bash test runner |
| Hayes harness in Forth | ~30 | external test deps |

**Result**: `./sixth test.fs` runs all tests. No bash anywhere.

### Phase 10: Bare Metal — FUTURE

See [SIXTH_OS.md](/SIXTH_OS.md).

| Task | Lines | Eliminates |
|------|-------|------------|
| Replace syscalls with VGA/keyboard | ~35 | Linux kernel |
| Block-based source loading | ~30 | filesystem |
| Remove ELF generation | -80 | ELF loader |
| colorForth integration | ~20 | BIOS/UEFI |

**Result**: Boot from BIOS. No Linux. No C. ~3000 lines from power-on to native code.

---

## Summary

| Phase | Lines | Eliminates | Status |
|-------|-------|------------|--------|
| 1. Parsing | ~100 | C lexer | DONE |
| 2. Dictionary | ~50 | C symbol table | NOT STARTED |
| 3. State | ~20 | C state machine | NOT STARTED |
| 4. Defining words | ~0 | C defining words | MOSTLY DONE |
| 5. Interpreter | ~185 | C REPL | NOT STARTED |
| 6. File I/O | ~70 | `cat`, pipes, heredocs | PARTIAL |
| 7. Cleanup | 0 | **GCC dependency** | NOT STARTED |
| 8. Native SQLite | ~100 | `sqlite3` shell-out | NOT STARTED |
| 9. Test framework | ~80 | bash test runner | NOT STARTED |
| 10. Bare metal | -15 | **Linux kernel** | FUTURE |

| Milestone | Lines | Depends On |
|-----------|-------|------------|
| Current | 2800 | C, bash, Linux |
| After Phase 7 | ~3125 | bash, Linux |
| After Phase 9 | ~3305 | Linux |
| After Phase 10 | ~3290 | **nothing** |

---

## Complexity Analysis

The compiler was the hard part. Everything else is easier.

### What's Done

| Task | Lines | Complexity | Notes |
|------|-------|------------|-------|
| Learn Forth | 0 | High | New paradigm, unlearn assumptions |
| Understand native compilation | 0 | High | Insight: Forth is already optimizable |
| Write native x86-64 compiler | 2800 | High | Stack caching, superinstructions, register allocation |

### What Remains

| Phase | Lines | Complexity | Why |
|-------|-------|------------|-----|
| No C (7) | ~325 | Medium | Known problem: FIND, EXECUTE, EVALUATE |
| No bash (8-9) | ~180 | Low | Plumbing, not insight |
| No Linux (10) | ~5 | **Trivial** | colorForth exists. Delete ELF code. Change 65 lines. |

### The Ratio

| Phase | % of compiler | Nature of work |
|-------|---------------|----------------|
| No C | 12% | Last real intellectual work (interpreter loop) |
| No bash | 6% | Mechanical (syscalls, test harness) |
| No Linux | 0.2% | **Removal** (ELF gone, syscalls simplified) |

### The Insight

Phase 10 (bare metal) is the *easiest* phase. You're not writing an OS — Chuck Moore already wrote colorForth in 2001. You're:

1. Deleting 80 lines of ELF generation
2. Replacing 65 lines of Linux syscalls with direct hardware calls
3. Done

The path to bare metal is shorter than the path already walked.

---

## Architecture

```
rax = TOS
rbx = NOS (when depth >= 2)
rcx = third (when depth >= 3)
r15 = data stack pointer (grows down)
r12 = do/loop index
r13 = do/loop limit
```

---

## Success Criteria

Sovereignty complete when:

1. `./sixth sixth.fs` produces working `sixth` binary
2. That binary can compile sixth.fs again (bootstrap)
3. `./sixth` starts interactive REPL
4. `rm -rf engine/` — no C required
5. Hayes Core tests pass
6. Zero shell-outs in any Sixth program

Ultimate sovereignty (Phase 10):

7. Boots from BIOS — no Linux
8. ~3000 lines from power-on to native code
9. Depends on nothing but electricity

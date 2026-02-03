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

~3240 lines of Forth. Compiles to x86-64 machine code. Single-pass.

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
./fifth compiler/tests/run.fs
```

1598 pass, 5 wrong, 50 skip. Single command.

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
[x] 9.  POSTPONE ( "name" -- )            compile compilation (user words only, not builtins)
[x] 10. DOES>    ( -- )                   set runtime behavior (direct CREATE usage only)
[x] 11. OPEN-FILE                         syscall wrapper
[x] 12. READ-FILE                         syscall wrapper
[ ] 13. READ-LINE                         line-by-line reading
[ ] 14. INCLUDE  ( "name" -- )            load and evaluate file
------- SELF-HOSTING COMPLETE -------
[ ] 15. Delete engine/                    remove C interpreter
[ ] 16. Forth test framework              replace bash test runner
------- SOVEREIGNTY COMPLETE -------
[x] 17. ARGC ARGV                         command line args
[ ] 18. CLOCK-MS                          syscall 228 (timing)
[ ] 19. GETENV                            environment variables
[ ] 20. BYE                               syscall 60 (clean exit)
[ ] 21. THROW CATCH                       exception handling
------- PRACTICAL COMPLETE -------
```

---

## Bootstrap Sequence

**How we eliminate the C interpreter:**

The native compiler (`sixth.fs`) is currently run by the C interpreter (`fifth`).
Once READ-LINE and INCLUDE are implemented, we can bootstrap:

### Step 1: Create Native Binary

```bash
./fifth compiler/sixth.fs compiler/sixth.fs ./sixth
#  │                            │              │
#  └── C interpreter            │              └── Output: native ELF
#      (engine/sixth.c)         └── Source being compiled
```

This produces `./sixth` — a standalone native executable.

### Step 2: Verify Self-Hosting

```bash
./sixth compiler/sixth.fs compiler/sixth.fs ./sixth2
#  │                                           │
#  └── Native binary (no C needed)             └── Should work identically
```

If `./sixth2` can also compile programs correctly, we've achieved self-hosting.

### Step 3: Delete C Interpreter

```bash
rm -rf engine/
```

The native `sixth` binary IS the Forth system. It's not interpreting — it's compiling
to machine code and executing that. But from the user's perspective, it does what
an interpreter does: takes Forth source, runs it.

### Dependency Chain After Bootstrap

```
Linux kernel
    └── sixth (native ELF binary, ~15KB)
            └── sixth.fs (source, optional after bootstrap)
```

No GCC. No C runtime. No bash. Just a binary that speaks Forth.

**Key insight**: "Delete engine/" doesn't mean we lose the interpreter. The compiled
`sixth` binary contains everything: tokenizer, parser, code generator, EVALUATE,
INCLUDE. It's a complete Forth system in native code.

---

### Missing Syscall Wrappers

Words in interpreter but not native compiler. Required for practical use.

| Word | Syscall | Purpose | Lines | Status |
|------|---------|---------|-------|--------|
| `argc` `argv` | N/A (kernel passes at startup) | Command line args | ~20 | **DONE** |
| `clock-ms` | 228 (clock_gettime) | Timing, benchmarks | ~15 | — |
| `getenv` | N/A (parse environ pointer) | Environment variables | ~25 | — |
| `bye` | 60 (exit) | Clean exit from REPL | ~5 | — |
| `throw` `catch` | N/A (stack manipulation) | Exception handling | ~40 | — |

**Notes:**
- `argc`/`argv`: **Done.** Prologue saves `[rsp]` and `[rsp+8]` to data segment.
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

### Phase 2: Dictionary — DONE

| Word | Status |
|------|--------|
| FIND | DONE |
| EXECUTE | DONE |
| ' | DONE |
| ['] | DONE |
| >BODY | DONE |

### Phase 3: State Machine — DONE

| Word | Status |
|------|--------|
| STATE | DONE |
| [ | DONE |
| ] | DONE |
| LITERAL | DONE |

### Phase 4: Defining Words — DONE

| Word | Status |
|------|--------|
| CREATE | DONE |
| : ; | DONE |
| IMMEDIATE | DONE |

### Phase 5: Interpreter Loop — DONE

| Word | Status |
|------|--------|
| INTERPRET | DONE |
| EVALUATE | DONE |
| ABORT | DONE |
| QUIT | DONE |
| POSTPONE | DONE (user words) |
| DOES> | DONE (direct usage) |

#### Implementation Order

Build in this exact order. Each word depends on the ones before it.

**1. INTERPRET — DONE**

**2. EVALUATE — DONE**

**3. ABORT — DONE**
- Simple: clear stacks, call QUIT
- Error recovery

**4. QUIT — DONE**
- Main REPL loop
- Calls REFILL, INTERPRET, repeats

**5. POSTPONE — DONE**
- Compiles code that compiles
- Used for defining control structures in Forth

**6. DOES> — DONE**
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

### Phase 6: File I/O — MOSTLY DONE

| Word | Status | Est. Lines |
|------|--------|------------|
| slurp-file | DONE (in interpreter) | — |
| write-file | DONE (for ELF output) | — |
| close-file | DONE | — |
| OPEN-FILE | DONE | — |
| READ-FILE | DONE | — |
| READ-LINE | NOT STARTED | ~20 |
| INCLUDE | NOT STARTED | ~20 |

~40 lines remaining.

#### Implementation Order

**7. OPEN-FILE — DONE**
- Syscall wrapper (open)
- Returns file handle

**8. READ-FILE — DONE**
- Syscall wrapper (read)
- Needs file handle from OPEN-FILE

**9. READ-LINE — next**
- Uses READ-FILE
- Scans for newline
- Used by INCLUDE for line-by-line loading

**10. INCLUDE — last**
- Opens file (OPEN-FILE)
- Reads lines (READ-LINE)
- Evaluates each line (EVALUATE)
- Closes file

This completes the interpreter. Hayes tests can run.

### Phase 7: Bootstrap & Cleanup — NOT STARTED

| Task | Purpose |
|------|---------|
| Bootstrap sixth | `./fifth sixth.fs sixth.fs ./sixth` — create native binary |
| Verify self-host | `./sixth sixth.fs sixth.fs ./sixth2` — must work |
| Delete engine/ | `rm -rf engine/` — C interpreter no longer needed |

See "Bootstrap Sequence" above for detailed steps.

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
| 2. Dictionary | ~50 | C symbol table | DONE |
| 3. State | ~20 | C state machine | DONE |
| 4. Defining words | ~0 | C defining words | DONE |
| 5. Interpreter | ~185 | C REPL | DONE |
| 6. File I/O | ~70 | `cat`, pipes, heredocs | MOSTLY DONE (~40 left) |
| 7. Bootstrap | 0 | **C interpreter** | NOT STARTED |
| 8. Native SQLite | ~100 | `sqlite3` shell-out | NOT STARTED |
| 9. Test framework | ~80 | bash test runner | NOT STARTED |
| 10. Bare metal | -15 | **Linux kernel** | FUTURE |

| Milestone | Lines | Depends On |
|-----------|-------|------------|
| Current | ~3240 | C interpreter (fifth), Linux |
| After Phase 6 | ~3280 | C interpreter (fifth), Linux |
| After Phase 7 | ~3280 | **Linux only** (native sixth binary) |
| After Phase 9 | ~3360 | Linux only |
| After Phase 10 | ~3345 | **nothing** |

---

## Complexity Analysis

The compiler was the hard part. Everything else is easier.

### What's Done

| Task | Lines | Complexity | Notes |
|------|-------|------------|-------|
| Learn Forth | 0 | High | New paradigm, unlearn assumptions |
| Understand native compilation | 0 | High | Insight: Forth is already optimizable |
| Write native x86-64 compiler | 3100 | High | Stack caching, superinstructions, register allocation |
| Interpreter loop | ~140 | Medium | INTERPRET, EVALUATE, QUIT, ABORT |
| File I/O syscalls | ~50 | Low | OPEN-FILE, READ-FILE, WRITE-FILE |

### What Remains

| Phase | Lines | Complexity | Why |
|-------|-------|------------|-----|
| File I/O (6) | ~40 | Low | READ-LINE, INCLUDE — known patterns |
| Bootstrap (7) | 0 | **Trivial** | Just run the compiler on itself |
| No bash (8-9) | ~180 | Low | Plumbing, not insight |
| No Linux (10) | ~5 | **Trivial** | colorForth exists. Delete ELF code. |

### The Ratio

| Phase | % of compiler | Nature of work |
|-------|---------------|----------------|
| File I/O | 1.2% | Mechanical (syscall wrappers) |
| Bootstrap | 0% | Run existing code, delete C |
| No bash | 5.5% | Mechanical (test harness) |
| No Linux | 0.2% | **Removal** (ELF gone, syscalls simplified) |

### The Insight

The remaining phases are *removal*, not addition:

**Phase 7 (Bootstrap)**: Not writing new code. Running existing code on itself, then deleting the C interpreter.

**Phase 10 (Bare metal)**: Not writing an OS — Chuck Moore already wrote colorForth in 2001. You're:
1. Deleting ~80 lines of ELF generation
2. Replacing ~65 lines of Linux syscalls with direct hardware calls
3. Done

The path to sovereignty is shorter than the path already walked.

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

### Self-Hosting (Phase 7)

1. `./fifth compiler/sixth.fs compiler/sixth.fs ./sixth` — C interpreter creates native binary
2. `./sixth compiler/sixth.fs compiler/sixth.fs ./sixth2` — native binary compiles itself
3. `./sixth2 compiler/sixth.fs compiler/tests/run.fs` — second-gen compiler passes tests
4. `rm -rf engine/` — C interpreter deleted, native binary is the system
5. `./sixth` starts interactive REPL (QUIT loop)

### Sovereignty (Phase 9)

6. `./sixth test.fs` — test framework in Forth, no bash
7. Hayes Core tests pass via Forth harness
8. Zero shell-outs in any Sixth program

### Ultimate Sovereignty (Phase 10)

9. Boots from BIOS — no Linux
10. ~3300 lines from power-on to native code
11. Depends on nothing but electricity

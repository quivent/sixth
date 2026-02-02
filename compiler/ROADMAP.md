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

## Self-Hosting Roadmap

Goal: Full interactive Forth system. Interprets, compiles, no C.

### Phase 1: Input Parsing — DONE

| Word | Status |
|------|--------|
| SOURCE | DONE |
| >IN | DONE |
| PARSE | DONE |
| WORD | DONE |
| ACCEPT | DONE |
| REFILL | DONE |

### Phase 2: Dictionary — NOT STARTED

| Word | Purpose | Est. Lines |
|------|---------|------------|
| FIND | ( addr u -- xt flag ) standard lookup | ~20 |
| ' | ( "name" -- xt ) parse and find | ~10 |
| ['] | ( "name" -- ) compile xt as literal | ~10 |
| EXECUTE | ( xt -- ) call execution token | ~5 |
| >BODY | ( xt -- addr ) get data field | ~5 |

~50 lines total.

### Phase 3: State Machine — NOT STARTED

| Word | Purpose | Est. Lines |
|------|---------|------------|
| STATE | ( -- addr ) compilation state variable | DONE (exists) |
| [ | ( -- ) switch to interpret mode | ~5 |
| ] | ( -- ) switch to compile mode | ~5 |
| LITERAL | ( n -- ) compile literal | ~10 |

~20 lines total.

### Phase 4: Defining Words — MOSTLY DONE

| Word | Purpose | Est. Lines |
|------|---------|------------|
| CREATE | DONE (basic form exists) | — |
| : ; | DONE | — |
| IMMEDIATE | DONE | — |

POSTPONE and DOES> moved to Phase 5 (depend on INTERPRET/EVALUATE).

### Phase 5: Interpreter Loop — NOT STARTED

| Word | Purpose | Est. Lines |
|------|---------|------------|
| INTERPRET | ( -- ) process input buffer | ~30 |
| EVALUATE | ( addr u -- ) interpret string | ~40 |
| ABORT | ( -- ) clear and restart | ~10 |
| QUIT | ( -- ) main interpreter loop | ~30 |
| POSTPONE | ( "name" -- ) compile compilation | ~15 |
| DOES> | ( -- ) set runtime behavior | ~60 |

~185 lines total.

#### Implementation Order

Build in this exact order. Each word depends on the ones before it.

**1. INTERPRET — first**
- Parses tokens from input buffer
- Looks up in dictionary (FIND exists)
- Executes or compiles based on STATE
- This is the core loop

**2. EVALUATE — second**
- Takes (addr u), saves current input state
- Sets new input buffer
- Calls INTERPRET
- Restores input state
- Needed for: compile-time execution, INCLUDE

**3. ABORT — third**
- Simple: clear stacks, call QUIT
- Error recovery

**4. QUIT — fourth**
- Main REPL loop
- Calls REFILL, INTERPRET, repeats
- Needs INTERPRET

**5. POSTPONE — fifth**
- Needs EVALUATE working to test properly
- Compiles code that compiles
- Used for defining control structures in Forth

**6. DOES> — sixth**
- Needs POSTPONE for clean implementation
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

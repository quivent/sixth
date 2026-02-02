# Sixth Native Compiler Roadmap

## Current State

~2600 lines of Forth. Compiles to x86-64 machine code. Single-pass.

### Compiled Words (105 total)

**Arithmetic**: `+ - * / mod /mod negate abs 1+ 1- 2+ 2- 2* 2/`
**Comparison**: `= <> < > 0= 0< 0<> <= >= u< u> within`
**Logic**: `and or xor invert`
**Stack**: `dup drop swap over rot nip tuck 2dup 2drop 2swap 2over ?dup depth pick`
**Control**: `if else then begin until while repeat do loop +loop i j leave recurse recursive exit`
**Memory**: `@ ! c@ c! +! cells cell+ variable constant create allot here , c,`
**Return Stack**: `>r r> r@ 2>r 2r> 2r@`
**I/O**: `. emit cr type ." s" count`
**Shifts**: `lshift rshift min max`
**Double**: `s>d um* m* um/mod sm/rem fm/mod d+ d-`
**Fused**: `nos+ nos- tuck+ dup+ dup- <if 1-nzloop dup2 0<if 0=if 0<>if`

### Optimizations Active

| Optimization | Status | Key Test | sixth/gcc-O2 |
|-------------|--------|----------|--------------|
| Stack caching (TOS in rax) | DONE | 100-dup-add | 1.48x |
| Superinstruction `dup+` | DONE | 100-dup-add | 1.48x |
| Branch fusion `<if` | DONE | 450-dup-gt-while | 1.07x |
| do/loop registers (r12/r13) | DONE | 614-doloop-basic | 1.17x |
| Constant folding | DONE | 1002-fold-mul | 1.29x |
| Literal-op fusion | DONE | 1019-fuse-and-imm | 1.31x |
| Tail-call (recurse->jmp) | DONE | 235-recurse-fact | 1.04x |
| Forward references | DONE | 1031-fwd-ref-chain | 1.20x |

### Testing

```bash
# Run all tests (1648 tests)
./sixth compiler/tests/run.fs
# Expected: 1597 pass, 0 wrong, 51 skip

# Single test
./sixth compiler/sixth.fs compiler/tests/01-lit.fs /tmp/t && /tmp/t

# Reference benchmark (must output 8189)
./sixth compiler/sixth.fs compiler/bench/ack.fs /tmp/ack && /tmp/ack
```

Do not modify ack.fs to make it pass. Fix the compiler.

---

## Self-Hosting Roadmap

Goal: sixth.fs compiles itself. No C. No dependencies.

### Phase 1: Self-Hosting (6 steps)

| Step | Feature | Est. Lines | Status |
|------|---------|------------|--------|
| 1 | Input parsing (SOURCE, >IN, WORD, PARSE, EVALUATE) | ~100 | NOT STARTED |
| 2 | Dictionary lookup (FIND, ', ['], EXECUTE, >BODY) | ~60 | NOT STARTED |
| 3 | CREATE DOES> | ~80 | NOT STARTED |
| 4 | STATE [ ] | ~20 | NOT STARTED |
| 5 | ACCEPT, ALIGN, ALIGNED, UNLOOP, HEX | ~65 | NOT STARTED |
| 6 | Delete C interpreter | 0 | NOT STARTED |

### Phase 2: Optimization (post self-hosting)

| Feature | Est. Lines | Status |
|---------|------------|--------|
| Inlining (<20 byte words) | ~50 | BLOCKED (dup-pending conflict) |

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

## Line Count

| Phase | Lines | Total |
|-------|-------|-------|
| Current | 2600 | 2600 |
| Self-hosting | ~325 | ~2925 |
| Inlining | ~50 | ~2975 |

**~3000 lines of Forth.** GCC: 15M lines. LLVM: 20M lines.

---

## Success Criteria

Self-hosting complete when:
1. `./sixth sixth.fs -o sixth` produces working binary
2. `./sixth` interprets and compiles interactively
3. Hayes ANS Forth test suite passes
4. `rm -rf engine/` - no C required

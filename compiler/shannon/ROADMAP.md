# Shannon Compiler Roadmap

## The Goal

**Compile speed faster than GCC -O2, runtime performance at parity or better.**

---

## Phase 1: Self-Hosting

**Objective:** Shannon compiles Shannon. Delete C interpreter.

### Requirements
- Port 12 critical handlers from sixth.fs to Shannon modules
- All handlers exist in sixth.fs, just need transplanting

### Critical Handlers Needed

| Word | Uses | Target Module | Status |
|------|------|---------------|--------|
| include | 65x | main.fs | **NOT STARTED** |
| move | 18x | prims.fs | ✓ DONE |
| throw | 17x | control.fs | ✓ DONE |
| bye | 12x | io.fs | ✓ DONE |
| fill | 9x | prims.fs | ✓ DONE |
| argv | 7x | io.fs | ✓ DONE |
| argc | 6x | io.fs | ✓ DONE |
| write-file | 4x | io.fs | ✓ DONE |
| create-file | 3x | io.fs | ✓ DONE |
| close-file | 3x | io.fs | ✓ DONE |
| w/o | 3x | compile.fs (constant) | ✓ DONE |
| slurp-file | 2x | io.fs | ✓ DONE |

### Verification
```bash
# Stage 1: Compile Shannon with C interpreter
./engine/fifth compiler/shannon/main.fs compiler/shannon/main.fs /tmp/stage1

# Stage 2: Compile Shannon with stage1
/tmp/stage1 compiler/shannon/main.fs /tmp/stage2

# Stage 3: Compile Shannon with stage2
/tmp/stage2 compiler/shannon/main.fs /tmp/stage3

# Verify: stage2 and stage3 must be identical
diff /tmp/stage2 /tmp/stage3 && echo "SELF-HOSTING ACHIEVED"
```

### Milestone
- [ ] All 12 handlers ported
- [ ] Stage 1 compiles
- [ ] Stage 2 compiles
- [ ] Stage 3 matches Stage 2
- [ ] C interpreter deleted

---

## Phase 2: Full Language Compiler

**Objective:** Shannon compiles ANY valid Forth program.

### Requirements
- Port remaining ~43 standard Forth handlers
- Complete ANS Forth core word set coverage

### Handler Categories

| Category | Count | Examples |
|----------|-------|----------|
| Double-cell arithmetic | 8 | d+ d- m* um* um/mod sm/rem fm/mod s>d |
| Scaled arithmetic | 2 | */ */mod |
| Extended stack | 6 | 2+ 2- cell+ dup2 nos+ tuck+ |
| String/parsing | 7 | count parse word accept source refill >in |
| Pictured numeric output | 2 | hold sign |
| Control extensions | 5 | abort quit evaluate interpret postpone |
| File I/O extensions | 3 | open-file read-file r/o r/w |
| Comparison extensions | 2 | u< within |
| Loop optimizations | 3 | 0=until nzloop 1-nzloop |
| Miscellaneous | 5 | base >body spaces find div |

### Milestone
- [ ] All ~55 handlers ported
- [ ] Hayes ANS Forth test suite passes
- [ ] Can compile arbitrary user programs

---

## Phase 3: Optimize

**Objective:** Compile faster than GCC -O2, runtime at parity or better.

### Benchmarks
- Compile time: Shannon vs `gcc -O2` on equivalent C
- Runtime: Shannon-compiled binary vs GCC-compiled binary
- Metrics: wall clock, instructions, cache misses

### Optimization Targets

| Area | Current | Goal |
|------|---------|------|
| Constant folding | Basic | Cross-word propagation |
| Literal fusion | add-imm, mul→shift | All immediate forms |
| Swap elimination | Pending swap | Full commutative absorption |
| Inlining | None | Small word inlining |
| Register allocation | Fixed (RAX/RBX/RCX) | Optimal allocation |
| Dead code elimination | None | Unreachable code removal |
| Tail call optimization | None | Jump instead of call |

### Milestone
- [ ] Benchmark suite established
- [ ] Compile time < GCC -O2
- [ ] Runtime within 10% of GCC -O2
- [ ] Runtime at parity with GCC -O2

---

## Current Status

**Phase 1: 12/12 handlers ported. NEW BLOCKER: `/string`**

| Handler | Status |
|---------|--------|
| move, fill, throw, bye | ✓ DONE |
| argc, argv | ✓ DONE |
| write-file, create-file, close-file | ✓ DONE |
| w/o, slurp-file | ✓ DONE |
| include | ✓ DONE |
| **/string** | **BLOCKING** - not in dispatch, used 4x in parse-number |

Self-hosting test result: `Unknown: /string`

---

## Architecture Notes

### Why Shannon over sixth.fs?

| Aspect | sixth.fs | Shannon |
|--------|----------|---------|
| Lines | 3,372 (monolith) | ~2,500 (16 modules) |
| Handlers | 189 | 91 |
| Modularity | None | Clean separation |
| Testability | Hard | Module-level testing |
| Optimization work | Risky | Isolated changes |

Shannon is the right foundation. It just needs the missing 12 handlers.

### Module Structure

```
asm.fs       - x86-64 instruction encoding
stack.fs     - Register-mapped stack machine
prims.fs     - Primitive codegen (pure, no optimization)
control.fs   - Control flow (if/then/else, loops)
io.fs        - I/O operations (emit, cr, type, file ops)
rstack.fs    - Return stack operations
opt-fold.fs  - Constant folding
opt-fuse.fs  - Literal fusion
opt-swap.fs  - Swap elimination
scan.fs      - Pass 1: Word metadata scanner
dispatch.fs  - Builtin lookup table
elf.fs       - ELF binary generation
defs.fs      - variable/constant/create
strings.fs   - String literals
compile.fs   - Compilation orchestration
main.fs      - Entry point and glue
```

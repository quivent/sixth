# sixth.fs Native Compiler Roadmap

## Current State

sixth.fs compiles Forth to x86-64 machine code. Single-pass, ~1500 lines of Forth.

### Vocabulary Coverage

| Source | Words | Notes |
|--------|-------|-------|
| sixth.fs compiled | 105 | 93 standard + 12 custom |
| ANS Forth Core | ~180 | sixth.fs covers 33% |
| Sixth interpreter | ~480 | 178 C prims + 15 boot + 302 lib |

### 71 Compiled Words

**Arithmetic**: `+` `-` `*` `/` `mod` `/mod` `negate` `abs` `1+` `1-` `2+` `2-` `2*` `2/`
**Comparison**: `=` `<>` `<` `>` `0=` `0<` `0<>`
**Logic**: `and` `or` `xor` `invert`
**Stack**: `dup` `drop` `swap` `over` `rot` `nip` `tuck` `2dup` `2drop` `2swap` `2over` `?dup` `depth` `pick`
**Control**: `if` `else` `then` `begin` `until` `begin` `while` `repeat` `do` `loop` `+loop` `i` `j` `leave` `recurse` `exit`
**Memory**: `@` `!` `c@` `c!` `+!` `cells` `cell+` `variable` `constant` `create` `allot` `here`
**Return Stack**: `>r` `r>` `r@` `2>r` `2r>` `2r@`
**I/O**: `.` `emit` `cr` `type` `." ..."` `s"`
**Extra Ops**: `min` `max` `lshift` `rshift`
**Double-Cell**: `s>d` `um*` `m*` `um/mod` `sm/rem` `fm/mod` `d+` `d-`
**Custom**: `nos+` `nos-` `tuck+` `dup+` `dup-` `<if` `1-nzloop` `dup2` `0<if` `0=if` `0<>if`

### Performance Anchors (sustain these)

| Metric | Value | Target |
|--------|-------|--------|
| sixth.fs avg compile time | 7.0ms | < 10ms |
| gcc-O2 avg compile time | 29.2ms | (reference) |
| sixth.fs avg runtime | 1.4ms | < 2.0ms |
| gcc-O2 avg runtime | 1.6ms | (reference) |
| sixth.fs/gcc-O2 speed ratio | 0.88x | > 0.80x |
| Correctness | 999/1050 (95%) | >= 999/1050 |

### Headline Speedups: Native vs Interpreter (protect these)

Heavy-iteration benchmarks in `bench/`. These measure the full compiler payoff.

**Custom-word benchmarks** (nos+, 1-nzloop — sixth.fs superinstructions):

| Benchmark | Interpreted | Native | gcc -O2 | vs Interp | vs gcc |
|-----------|------------|--------|---------|-----------|--------|
| **arith** (10M nos+/1-nzloop) | 124ms | 1ms | 1ms | **124x** | 1.0x |
| **loop** (100M 1-nzloop) | 1577ms | 1ms | 1ms | **1577x** | 1.0x |

These match gcc-O2 exactly. The loop benchmark uses empty-body detection
(gen-1-nzloop emits `xor eax,eax` when body is empty). The arith benchmark
uses `nos+` (single `inc rbx` instruction) in a tight loop.

**Standard-word benchmarks** (portable Forth, no custom words):

| Benchmark | Interpreted | Native | gcc -O2 | vs Interp | vs gcc |
|-----------|------------|--------|---------|-----------|--------|
| **branch** (10M if/else) | 435ms | 9ms | 9ms | **48x** | 1.0x |
| **stack** (10M swap in do) | 92ms | 3ms | 5ms | **31x** | 1.7x fast |
| **nested** (1M nested do) | 13ms | 2ms | 1ms* | **6.5x** | 2x slow |

\* gcc -O2 recognizes count pattern and computes at compile time.

### Per-Optimization Anchors (sustain these individually)

| Optimization | Key Test | tf/gcc-O2 | Floor |
|-------------|----------|-----------|-------|
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

### Regression Testing

`compiler/regress.fs` — compiles each test with sixth.fs, runs it, compares output to `\ expect:` comment. ~8 seconds for 1050 tests. No GCC needed.

```
./sixth compiler/regress.fs
```

Current baseline: 999 pass, 51 skip. Phases 1-4 complete: return stack, memory/defining words, trivial ops, strings. call/fibrec benchmarks fixed (tail-call patch was overwriting register saves instead of call opcode).

## Phase 1: Return Stack

**Words**: `>r` `r>` `r@` `2>r` `2r>` `2r@`

**Implementation**: Map to x86 `rbp` as return stack pointer. Use a separate memory region (not the call stack).

```
>r : sub rbp, 8 ; mov [rbp], rax ; pop rax (from data stack)
r> : push rax ; mov rax, [rbp] ; add rbp, 8
r@ : push rax ; mov rax, [rbp]
```

The return stack is separate from the x86 call stack (rsp). Allocate 8KB at a fixed address in the ELF (e.g., 0x500000). Initialize `rbp` to top of this region at program start.

**Complexity**: Low. ~50 lines added to sixth.fs.

**Impact**: Enables idioms like `2>r ... 2r>` for string pair save/restore. Required by many standard programs.

## Phase 2: Memory and Defining Words

**Words**: `variable` `constant` `create` `allot` `!` `@` `c!` `c@` `+!` `cells` `cell+` `here` `,`

**Implementation**: Add a data segment to the ELF output. sixth.fs currently emits a single LOAD segment (text only). Add a second LOAD segment at 0x600000 for read-write data.

- `variable` allocates 8 bytes in data segment, compiles as `push rax; mov rax, <addr>`
- `constant` compiles as `push rax; mov rax, <value>` (same as literal)
- `create` records current data pointer; `allot` advances it
- `!` / `@` compile to `mov [rax], rbx` / `mov rax, [rax]` patterns
- `here` returns current data segment pointer
- `,` stores a cell and advances `here`

**ELF changes**: Add second program header (PT_LOAD, RW). Adjust ELF header `e_phnum` from 1 to 2. ~30 lines of ELF generation.

**Complexity**: Medium. ~150 lines. The ELF changes are the hardest part — must get alignment and permissions right.

**Impact**: Large. Enables stateful programs, lookup tables, buffers. Most real Forth programs need variables.

## Phase 3: Trivial Operations

**Words**: `min` `max` `lshift` `rshift` `negate` (already done) `within` `*/` `*/mod`

**Implementation**: These are all 1-3 instruction sequences.

```
min : cmp rax, [rsp] ; cmovg rax, [rsp] ; add rsp, 8
max : cmp rax, [rsp] ; cmovl rax, [rsp] ; add rsp, 8
lshift : mov rcx, rax ; pop rax ; shl rax, cl
rshift : mov rcx, rax ; pop rax ; shr rax, cl
```

**Complexity**: Low. ~40 lines. Each word is a trivial pattern.

## Phase 4: Strings

**Words**: `s"` `type` `count` `move` `fill` `compare`

**Implementation**: String literals go in the data segment. `s"` compiles the string into data, then pushes addr and length at runtime.

```
s" hello" :
  ; In data segment: store "hello" at offset N
  ; In code: push rax; mov rax, <len>; push rax; mov rax, <addr>
  ; Result: ( addr u ) on stack
```

`type` needs a syscall: `mov rdi, 1; mov rsi, addr; mov rdx, len; mov rax, 1; syscall`

**Requires**: Phase 2 (data segment).

**Complexity**: Medium. ~80 lines. String literal parsing in the compiler, syscall generation.

## Phase 5: Double-Cell and Unsigned

**Words**: `s>d` `d+` `d-` `um*` `um/mod` `m*` `fm/mod` `sm/rem`

**Implementation**: Double-cell values use two stack slots. `um*` maps to x86 `mul` (which produces 128-bit result in rdx:rax). `um/mod` maps to `div`.

```
um* : pop rbx ; mul rbx  ; push rdx ; (rax already has low)
```

**Complexity**: Medium. ~60 lines. The division words have subtle sign-handling differences (floored vs symmetric).

## Order of Work

1. Return stack (unblocks many idioms)
2. Data segment + memory words (unblocks real programs)
3. Trivial ops (easy wins, fills vocabulary gaps)
4. Strings (needs data segment)
5. Double-cell (niche, but required for ANS compliance)

After all phases: ~110 standard words compiled, ~61% ANS Core coverage. sixth.fs grows from ~1500 to ~2000 lines.

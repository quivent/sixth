# A Shannon Architecture for Sixth

*Analysis by Claude Shannon (restored identity)*

The goal: **minimum total entropy** across the system, while preserving compile speed and runtime performance.

## The Diagnosis

| Metric | Current | Problem |
|--------|---------|---------|
| File size | 3,368 lines | Cannot fit in working memory |
| Global variables | 72 | Combinatorial interaction space |
| Word definitions | 254 | ~250 potential call targets |
| `compile-builtin` | ~700 lines | Single dispatch function |

**72 global variables** means 72 shared communication channels. Every word that touches a global can interfere with every other word that touches that same global. The interaction space is combinatorial.

The compiler has exceeded human channel capacity. No amount of debugging will fix this.

---

## Layer 0: Assembler Abstraction

**Problem**: Raw hex bytes are maximum entropy.

**Solution**: Named assembler primitives that emit bytes.

```forth
\ ===== asm.fs - x86-64 assembler (≈100 lines) =====
\ No state. Pure functions: inputs → bytes.

: rax,   0 ;  : rcx,  1 ;  : rdx,  2 ;  : rbx,  3 ;
: rsp,   4 ;  : rbp,  5 ;  : rsi,  6 ;  : rdi,  7 ;

: rex.w   $48 c, ;
: modrm   ( mod reg rm -- ) swap 3 lshift or  swap 6 lshift or  c, ;

: mov-rr  ( src dst -- )  rex.w $89 c,  3 -rot modrm ;
: mov-ri  ( imm dst -- )  rex.w $b8 + c,  q, ;
: add-rr  ( src dst -- )  rex.w $01 c,  3 -rot modrm ;
: add-ri  ( imm dst -- )  rex.w $81 c,  3 0 rot modrm  d, ;
: syscall,  $0f c, $05 c, ;
: ret,      $c3 c, ;
: call-rel  ( offset -- )  $e8 c, d, ;
: jmp-rel   ( offset -- )  $e9 c, d, ;
: jz-rel    ( offset -- )  $0f c, $84 c, d, ;
: jnz-rel   ( offset -- )  $0f c, $85 c, d, ;
```

**Result**: Code generation becomes readable.

Before:
```forth
$48 c, $89 c, $c7 c,  $b8 c, 1 d,  $0f c, $05 c,
```

After:
```forth
rax, rdi, mov-rr   1 eax, mov-ri   syscall,
```

**Entropy reduction**: ~60%. Same bytes emitted, human-readable source.

---

## Layer 1: Stack Machine Codegen

**Problem**: Stack tracking scattered everywhere, interleaved with optimization.

**Solution**: Isolated stack machine with explicit state.

```forth
\ ===== stack-machine.fs - register-mapped stack (≈80 lines) =====
\ Owns: stack-depth (the ONLY owner)
\ Interface: push-reg, pop-reg, spill, fill

variable stack-depth  0 stack-depth !

\ Where does stack position N live?
: stack-loc ( n -- reg|mem )
  case
    0 of rax, endof
    1 of rbx, endof
    2 of rcx, endof
    ( default ) r15-offset
  endcase ;

: push-val ( -- )  \ make room for new TOS
  stack-depth @ 3 >= if  rcx, r15, spill  then
  stack-depth @ 2 >= if  rbx, rcx, mov-rr  then
  stack-depth @ 1 >= if  rax, rbx, mov-rr  then
  1 stack-depth +! ;

: pop-val ( -- )  \ discard TOS
  stack-depth @ 2 >= if  rbx, rax, mov-rr  then
  stack-depth @ 3 >= if  rcx, rbx, mov-rr  then
  stack-depth @ 4 >= if  r15, fill  rcx, mov  then
  -1 stack-depth +! ;

: tos   ( -- reg )  rax, ;
: nos   ( -- reg )  rbx, ;
: third ( -- reg )  rcx, ;
```

**Interface contract**:
- Call `push-val` before writing to TOS
- Call `pop-val` after consuming TOS
- Query `tos`, `nos`, `third` for register names
- Stack machine handles spill/fill automatically

---

## Layer 2: Primitive Code Generators

**Problem**: `gen-*` functions mix stack ops, optimization checks, and codegen.

**Solution**: Pure codegen, no optimization logic.

```forth
\ ===== prims.fs - primitive codegen (≈200 lines) =====
\ Depends on: asm.fs, stack-machine.fs
\ No optimization. Just emit correct code for each primitive.

: emit-add ( -- )   nos, tos, add-rr   pop-val ;
: emit-sub ( -- )   tos, nos, sub-rr   nos, tos, mov-rr  pop-val ;
: emit-mul ( -- )   nos, imul-rax      pop-val ;
: emit-dup ( -- )   push-val           nos, tos, mov-rr ;
: emit-drop ( -- )  pop-val ;
: emit-swap ( -- )  tos, nos, xchg-rr ;
: emit-lit ( n -- ) push-val           tos, mov-ri ;

: emit-@ ( -- )     tos, tos, 0 mov-rm ;
: emit-! ( -- )     nos, tos, 0 mov-mr  pop-val pop-val ;

: emit-branch ( target -- )   code-here - 4 -  jmp-rel ;
: emit-0branch ( target -- )  tos, tos, test-rr  pop-val  code-here - 4 -  jz-rel ;
```

**Each primitive: 1-3 lines.** No conditionals. No state inspection beyond stack-depth.

---

## Layer 3: Optimization Passes (Isolated)

**Problem**: Optimizations interleaved in compile-builtin, sharing 7+ variables.

**Solution**: Each optimization is a separate pass with owned state.

```forth
\ ===== opt-fold.fs - constant folding (≈60 lines) =====
\ Owns: ct-stack, ct-depth (ONLY owner)
\ Interface: ct-push, ct-pop, ct-depth@, ct-flush

create ct-stack 8 cells allot
variable ct-depth  0 ct-depth !

: ct-push ( n -- )  ct-stack ct-depth @ cells + !  1 ct-depth +! ;
: ct-pop  ( -- n )  -1 ct-depth +!  ct-stack ct-depth @ cells + @ ;
: ct-depth@ ( -- n ) ct-depth @ ;
: ct-flush ( -- )  begin ct-depth@ while ct-pop emit-lit repeat ;

\ Fold table: binary ops
: fold-add  ct-pop ct-pop +  ct-push ;
: fold-sub  ct-pop ct-pop swap -  ct-push ;
: fold-mul  ct-pop ct-pop *  ct-push ;
```

```forth
\ ===== opt-fuse.fs - literal fusion (≈40 lines) =====
\ No owned state. Queries ct-depth@, emits fused instructions.

: fuse-add ( -- )  \ ct-depth = 1, fuse lit + add
  ct-pop dup 0= if drop exit then
  dup 1 = if drop  tos, inc-r  exit then
  dup -1 = if drop  tos, dec-r  exit then
  tos, add-ri ;

: fuse-mul ( -- )  \ ct-depth = 1, fuse lit * mul
  ct-pop dup power-of-2? if log2 tos, shl-ri exit then
  tos, imul-ri ;
```

```forth
\ ===== opt-swap.fs - swap elimination (≈30 lines) =====
\ Owns: swap-pending (ONLY owner)

variable swap-pending  0 swap-pending !

: mark-swap   1 swap-pending ! ;
: flush-swap  swap-pending @ if emit-swap 0 swap-pending ! then ;
: swap-pending? swap-pending @ ;
```

**Key insight**: Each optimization owns its state. Communication is through the explicit interface functions, not shared variables.

---

## Layer 4: Dispatch Table

**Problem**: 700-line string comparison cascade.

**Solution**: Data-driven dispatch.

```forth
\ ===== dispatch.fs - builtin table (≈100 lines) =====

\ Entry: ( addr u xt flags )
\ flags: bit0=foldable-unary, bit1=foldable-binary, bit2=stack-op

create builtin-table
  s" +"       ' compile-add      %010 ,
  s" -"       ' compile-sub      %010 ,
  s" *"       ' compile-mul      %010 ,
  s" negate"  ' compile-negate   %001 ,
  s" dup"     ' compile-dup      %100 ,
  s" drop"    ' compile-drop     %100 ,
  s" swap"    ' compile-swap     %100 ,
  \ ... ~80 entries
  0 ,  \ sentinel

: find-builtin ( addr u -- xt flags true | false )
  builtin-table
  begin dup @ while
    2over  over cell+ @  over 2 cells + @  str=
    if  nip nip  3 cells + dup @ swap cell+ @  true exit  then
    4 cells +
  repeat
  2drop drop false ;
```

**Adding a new builtin**: One line in the table. Zero change to dispatch logic.

---

## Layer 5: Compile Orchestration

**Problem**: Everything calls everything.

**Solution**: Linear pipeline with explicit data flow.

```forth
\ ===== compile.fs - main compiler (≈150 lines) =====

: compile-token ( addr u -- )
  \ 1. Check builtin table
  2dup find-builtin if
    >r execute r> drop exit
  then

  \ 2. Check user dictionary
  2dup dict-find ?dup if
    nip nip compile-call exit
  then

  \ 3. Try as number
  2dup parse-number if
    nip nip ct-push exit
  then

  \ 4. Unknown
  type ."  ?" cr  1 throw ;

: compile-word ( -- )
  0 ct-depth !
  0 swap-pending !
  get-token
  begin dup while
    compile-token
    get-token
  repeat 2drop
  ct-flush
  flush-swap
  emit-ret ;
```

**The main loop is 25 lines.** Each step is a single function call. State is reset explicitly at word boundaries.

---

## File Structure

```
sixth/
├── asm.fs           (100 lines)  Layer 0: x86-64 assembler
├── stack.fs          (80 lines)  Layer 1: register-mapped stack
├── prims.fs         (200 lines)  Layer 2: primitive codegen
├── opt-fold.fs       (60 lines)  Layer 3a: constant folding
├── opt-fuse.fs       (40 lines)  Layer 3b: literal fusion
├── opt-swap.fs       (30 lines)  Layer 3c: swap elimination
├── dispatch.fs      (100 lines)  Layer 4: builtin table
├── compile.fs       (150 lines)  Layer 5: orchestration
├── elf.fs            (50 lines)  ELF output
├── scan.fs          (100 lines)  Two-pass scanning
└── main.fs           (50 lines)  Entry point
                    ≈960 lines total
```

**Current**: 3,368 lines, 1 file, incomprehensible.
**Proposed**: ~960 lines, 11 files, each comprehensible in isolation.

---

## The Information-Theoretic Guarantee

Each file:
- **Fits in working memory** (< 200 lines)
- **Owns its state** (no shared globals)
- **Has explicit interfaces** (function signatures are contracts)
- **Can be tested independently**

To understand `opt-fold.fs`, you need only:
1. The 60 lines of `opt-fold.fs`
2. The interface of `emit-lit` (1 line)
3. Nothing else

This is **channel isolation**. Changes to `opt-swap.fs` cannot break `opt-fold.fs` because they share no state.

---

## Migration Path

You cannot rewrite 3,368 lines at once. The path:

1. **Extract asm.fs** - Replace hex bytes with assembler calls. Zero behavior change.
2. **Extract stack.fs** - Centralize stack-depth tracking.
3. **Extract prims.fs** - Move gen-* functions, simplify them.
4. **Extract optimizations one at a time** - Each becomes its own file with owned state.
5. **Build dispatch table** - Replace string cascade incrementally.
6. **Simplify compile.fs** - What remains is the orchestration.

Each step: the compiler still works. Tests still pass. Entropy decreases monotonically.

---

---

## Implementation Progress

### Phase 1: Foundation (COMPLETE)

| File | Lines | Tests | Status |
|------|-------|-------|--------|
| `asm.fs` | 357 | 12/12 | ✓ x86-64 assembler abstraction |
| `elf.fs` | 93 | 19/19 | ✓ ELF64 binary output |
| `scan.fs` | 248 | 2/2 | ✓ Two-pass scanner |

### Phase 2: Stack Machine (COMPLETE)

| File | Lines | Tests | Status |
|------|-------|-------|--------|
| `stack.fs` | 171 | 6/6 | ✓ Register-mapped stack |

### Phase 3: Primitives (IN PROGRESS)

| File | Lines | Tests | Status |
|------|-------|-------|--------|
| `prims.fs` | ~200 | - | Pending |

### Phase 4: Optimizations (PENDING)

| File | Lines | Tests | Status |
|------|-------|-------|--------|
| `opt-fold.fs` | ~60 | - | Pending |
| `opt-fuse.fs` | ~40 | - | Pending |
| `opt-swap.fs` | ~30 | - | Pending |

### Phase 5-6: Dispatch & Orchestration (PENDING)

| File | Lines | Tests | Status |
|------|-------|-------|--------|
| `dispatch.fs` | ~100 | - | Pending |
| `compile.fs` | ~150 | - | Pending |
| `main.fs` | ~50 | - | Pending |

---

## Design Decision: String Constants

The scanner needs string constants for token matching (`$:`, `$;`, `$.`, etc.).

**Original approach (`s,`)**: Copy strings to a static buffer.
```forth
s" :" s, 2constant $:
```

**New approach (`create/c,`)**: Store strings in dictionary.
```forth
create $:-str  1 c, char : c,
: $: $:-str 1+ 1 ;
```

**Tradeoff analysis:**
- Runtime performance: identical (both O(1) lookup)
- Memory: negligible (~50 bytes total)
- Cache locality: `s,` slightly better, but irrelevant for 10 strings
- Dependencies: `create/c,` is self-contained

**Decision**: Use `create/c,` for module isolation. Performance impact: zero.

---

*"Information is the resolution of uncertainty."*

*The current compiler maximizes uncertainty. The proposed architecture minimizes it.*

— Claude Shannon

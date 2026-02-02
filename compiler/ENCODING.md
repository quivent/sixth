# COMPILER ENCODING

## DATA STRUCTURES

| Name | Purpose | Layout | Size |
|------|---------|--------|------|
| `code-buf` | Machine code output | Linear byte buffer | 4096 bytes |
| `dict-buf` | Compile-time dictionary | `name[24] + addr[4] + flags[4]` per entry | 256 entries x 32 bytes |
| `ct-stack` | Compile-time constant stack | 8 cells for pending literals | 64 bytes |
| `cf-stack` | Control flow stack | Forward reference addresses | 64 cells |
| `info-buf` | Word info (pass 1) | `name[24] + nargs[4] + flags[4]` per entry | 64 entries x 32 bytes |

## STATE VARIABLES

| Name | Tracks | Values |
|------|--------|--------|
| `code-pos` | Emission point in code-buf | 0..4096 |
| `stack-depth` | Virtual stack depth | 0=empty, 1=rax, 2=+rbx, 3=+rcx, 4+=memory |
| `ct-depth` | Pending compile-time constants | 0..8 |
| `state` | Compile vs interpret | 0=interpret, 1=compile |
| `swap-pending` | Deferred swap | 0=no, 1=yes |
| `dup-pending` | Deferred dup | 0=no, 1=yes |
| `cmp-pending` | Deferred comparison | 0=none, 1=0=, 2=0>, 3=0< |
| `do-depth` | DO/LOOP nesting | 0..8 |
| `has-io` | Current word has I/O | 0/1 |
| `arg-count` | Input args from stack comment | 0..15 |

## COMPILATION FLOW

1. **Load**: `slurp-file` into `input-buf`, set `input-len`
2. **Scan**: `scan-all` walks source, builds `info-buf` with word names and arg counts (forward ref resolution)
3. **Emit prologue**: `gen-prologue` sets r15 (data stack) and rbp (return stack), jumps over definitions
4. **Compile**: `compile-all` loops `get-token` / `compile-word`; literals go to ct-stack, words emit x86 or flush constants first
5. **Finalize**: `patch-start` resolves jump, emit startup code (base=10, dict init), call main, `gen-epilogue` (syscall exit), `write-elf`

## KEY WORDS

| Word | Stack | Purpose | Calls/Modifies |
|------|-------|---------|----------------|
| `c,` | `( b -- )` | Emit byte to code-buf | `code-pos` |
| `d,` | `( d -- )` | Emit 32-bit little-endian | `c,` x4 |
| `q,` | `( q -- )` | Emit 64-bit | `d,` x2 |
| `push-tos` | `( -- )` | Spill stack for new TOS | `stack-depth`, emits mov chain |
| `pop-tos` | `( -- )` | Restore TOS after consume | `stack-depth` |
| `gen-lit` | `( n -- )` | Emit literal load | `push-tos`, emits mov rax,imm |
| `ct-push` | `( n -- )` | Push to compile-time stack | `ct-stack`, `ct-depth` |
| `ct-flush` | `( -- )` | Emit all pending constants | `gen-lit` for each |
| `flush-swap` | `( -- )` | Emit deferred swap if pending | `flush-cmp`, `gen-swap` |
| `compile-builtin` | `( a u -- f )` | Handle ~100 Forth words | `gen-*` words |
| `compile-token` | `( a u -- )` | Compile one word | `dict-find`, `compile-builtin`, `parse-number` |
| `gen-call` | `( addr -- )` | Emit call, save/restore regs | `call-nargs`, `stack-depth` |
| `gen-if` | `( -- orig )` | Emit test+jz, return patch addr | `pop-tos`, `cf-push` |
| `gen-then` | `( orig -- )` | Patch forward jump | `patch-rel32` |
| `gen-while-fused` | `( dest cmp -- orig dest )` | Fused dup+cmp+while | `cmp-pending` |
| `gen-repeat` | `( orig dest -- )` | Close while loop, may eliminate | Byte pattern matching |
| `start-def` | `( a u -- )` | Begin : definition | `dict-add`, `parse-stack-comment` |
| `end-def` | `( -- )` | Close definition | `gen-ret` or tail-call patch |
| `scan-all` | `( -- )` | Pass 1: build word info | `info-buf` |
| `write-elf` | `( a u -- )` | Output binary | `elf-header`, file I/O |

## CODE GENERATION

**Emission point**: `code-buf + code-pos`. All `gen-*` words emit directly via `c,`, `d,`, `q,`.

**Instruction encoding**: Raw x86-64 bytes. Pattern: REX prefix ($48/$49), opcode, ModR/M, optional imm. Examples:
- `$48 c, $89 c, $c3 c,` = `mov rbx, rax`
- `$48 c, $b8 c, q,` = `mov rax, imm64`
- `$e8 c, d,` = `call rel32`

**Stack caching**: Literals accumulate in `ct-stack` (up to 8 deep). On non-foldable operation, `ct-flush` emits all as `mov rax, imm` with `push-tos` between. Binary ops with 2+ constants fold at compile time. Binary ops with 1 constant fuse: `3 +` becomes `add rax, 3` (7 bytes) not push+add+pop (~15 bytes).

## REGISTER ALLOCATION

| Register | Role |
|----------|------|
| rax | TOS (top of stack) |
| rbx | NOS (2nd) |
| rcx | 3rd |
| r15 | Data stack pointer (deeper items) |
| rbp | Return stack pointer |
| r12 | DO/LOOP index (i) |
| r13 | DO/LOOP limit |
| rdi, rsi, rdx | Scratch for syscalls, string ops |

**Spills**: When `stack-depth >= 4`, `push-tos` chains: save rcx to [r15], rbx to rcx, rax to rbx, then load new rax. Reverse on `pop-tos`. Memory traffic only for stack depth > 3.

## THE TRICK

The compiler is a **single-pass streaming translator** that defers decisions. Literals accumulate on `ct-stack` instead of generating code immediately. When a consumer word arrives, it checks `ct-depth`: if 2+, fold both constants at compile time; if 1, fuse into an immediate instruction; if 0, emit normal two-operand code. Similarly, `swap`, `dup`, and comparisons are deferred (`*-pending` variables) so they can cancel (`swap swap` = nop) or fuse with the next operation (`dup 0> while` = single conditional branch). Control flow uses a separate `cf-stack` for forward references, patched when targets are known. The double-pass scan (`scan-all`) exists only to count arguments for forward-referenced words. This architecture transforms naive stack code into register-efficient x86 with zero intermediate representation--the source *is* the IR, and the ct-stack *is* the optimizer state.

# Sixth Compiler Encoding

## DATA STRUCTURES

| Name | Purpose | Layout | Size |
|------|---------|--------|------|
| `code-buf` | Machine code output | Linear byte buffer | 256KB |
| `dict-buf` | Compile-time dictionary | `name[24] + addr[4] + flags[4]` per entry | 512 × 32B |
| `info-buf` | Two-pass word metadata | `name[24] + nargs[1] + rets[1] + flags[2] + call-count[2] + body-pos[4] + code-addr[4] + pad[2]` | 64 × 40B |
| `ct-stack` | Compile-time constant stack | Pending literals for folding | 8 cells |
| `cf-stack` | Control flow stack | Forward/backward jump targets | 64 cells |

## STATE VARIABLES

| Name | Tracks | Values |
|------|--------|--------|
| `code-pos` | Emission point in code-buf | 0..CODE-SIZE |
| `stack-depth` | Virtual stack depth | 0=empty, 1=rax, 2=+rbx, 3=+rcx, 4+=memory |
| `ct-depth` | Pending compile-time constants | 0..8 |
| `state` | Compile vs interpret | 0=interpret, 1=compile |
| `swap-pending` | Deferred swap | 0 or 1 |
| `dup-pending` | Deferred dup | 0 or 1 |
| `cmp-pending` | Deferred comparison | 0=none, 1=0=, 2=0>, 3=0< |
| `do-depth` | DO..LOOP nesting level | 0..8 |
| `input-pos` | Parser position | 0..input-len |

## COMPILATION FLOW

1. **Load** → `load-file` reads source into `input-buf`
2. **Scan** → `scan-all` builds `info-buf` (names, stack effects, call counts, body positions)
3. **Reset** → Zero `code-pos`, `dict-count`, `input-pos`
4. **Compile** → `compile-all` calls `compile-word` → `compile-token` for each token
5. **Emit** → `elf-header` + `write-elf` produces executable

## KEY WORDS

| Word | Stack | Purpose |
|------|-------|---------|
| `c,` | `( b -- )` | Emit byte to code-buf |
| `d,` | `( d -- )` | Emit 32-bit little-endian |
| `q,` | `( q -- )` | Emit 64-bit little-endian |
| `push-tos` | `( -- )` | Shift stack down: rax→rbx→rcx→[r15], inc depth |
| `pop-tos` | `( -- )` | Shift stack up: [r15]→rcx→rbx→rax, dec depth |
| `gen-lit` | `( n -- )` | Push constant: `push-tos` + `mov rax, n` |
| `ct-push` | `( n -- )` | Push to compile-time constant stack |
| `ct-flush` | `( -- )` | Emit all pending constants via `gen-lit` |
| `flush-swap` | `( -- )` | Emit deferred swap/dup/cmp if pending |
| `gen-call` | `( addr -- )` | Emit `call rel32`, save/restore rbx/rcx if needed |
| `gen-if` | `( -- orig )` | Emit `test rax; jz rel32`, return patch addr |
| `gen-then` | `( orig -- )` | Patch forward jump to current position |
| `gen-begin` | `( -- dest )` | Return code-here as loop target |
| `gen-until` | `( dest -- )` | Emit conditional backward jump |
| `compile-token` | `( a u -- )` | Dispatch: info-buf → dict → builtins → number → fixup |
| `compile-builtin` | `( a u -- f )` | Match ~170 primitives, emit optimized code |
| `start-def` | `( a u -- )` | Begin `:` definition, parse stack comment |
| `end-def` | `( -- )` | End definition, emit ret or tail-call |
| `scan-all` | `( -- )` | Pass 1: populate info-buf with all definitions |
| `info-find` | `( a u -- e\|0 )` | Lookup word in info-buf |

## CODE GENERATION

**Emission**: `c,` writes bytes to `code-buf[code-pos]`, increments `code-pos`.

**Encoding**: Raw x86-64 bytes as hex literals.
```forth
: gen-add ( -- )  $48 c, $01 c, $d8 c,  pop-nos ;  \ add rax, rbx
```

**Stack caching**: Depth 1–3 live in registers. Depth ≥4 spills to memory at [r15]. `push-tos` cascades: rax→rbx→rcx→[r15]. `pop-tos` promotes: [r15]→rcx→rbx→rax.

## REGISTER ALLOCATION

| Register | Role |
|----------|------|
| rax | TOS (top of stack) |
| rbx | NOS (second) |
| rcx | Third stack item |
| r15 | Data stack pointer (depth ≥4) |
| rbp | Return stack pointer |
| r12 | DO..LOOP index (I) |
| r13 | DO..LOOP limit |
| rdi/rsi/rdx | Scratch for syscalls, string ops |

**Spill**: When depth > 3, `push-tos` stores rcx to [r15], decrements r15. Promotion reverses this.

## THE TRICK

**Two-pass + deferred evaluation**. Pass 1 scans all definitions to know stack effects and call counts before emitting any code. Pass 2 uses this to: (1) resolve forward references with correct nargs for register save/restore, (2) inline words called exactly once (call-count=1, non-recursive), (3) fold constants across word boundaries. Deferred swap/dup/cmp enables fusion: `dup 0= while` becomes a single conditional jump without materializing the boolean.

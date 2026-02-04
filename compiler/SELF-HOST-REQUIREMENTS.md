# Self-Hosting Forth Compiler Requirements

First principles specification for a self-hosting Forth-to-x86-64 native compiler.

## Layer 1: Core Execution Model

| Requirement | Description |
|-------------|-------------|
| Stack representation | How the Forth data stack maps to CPU (registers, memory, hybrid) |
| Calling convention | How words call other words and return |
| Entry point | How `main` gets invoked at program start |
| Exit mechanism | Syscall to terminate cleanly |

## Layer 2: Primitives

| Category | Words |
|----------|-------|
| Stack ops | `dup` `drop` `swap` `over` `rot` `nip` `tuck` `2dup` `2drop` `2swap` |
| Arithmetic | `+` `-` `*` `/` `mod` `/mod` `negate` `abs` `1+` `1-` |
| Memory | `@` `!` `c@` `c!` `+!` |
| Comparison | `=` `<>` `<` `>` `<=` `>=` `0=` `0<` `0>` |
| Logic | `and` `or` `xor` `invert` `lshift` `rshift` |

## Layer 3: Definitions

| Requirement | Description |
|-------------|-------------|
| `: name ... ;` | Define a new word |
| `variable name` | Allocate storage, word returns address |
| `constant name` | Compile-time value |
| `create name` | Create a word that returns its data address |
| `does>` | Define runtime behavior for created words |
| `' name` | Get execution token (address) of word |
| `['] name` | Compile-time tick |
| `execute` | Call word given its XT |

## Layer 4: Control Flow

| Requirement | Description |
|-------------|-------------|
| `if` / `then` | Conditional |
| `if` / `else` / `then` | Conditional with alternative |
| `begin` / `again` | Infinite loop |
| `begin` / `until` | Loop until condition true |
| `begin` / `while` / `repeat` | Loop while condition true |
| `do` / `loop` | Counted loop (optional for bootstrap) |
| `recurse` | Self-recursion |
| `exit` | Early return from word |

## Layer 5: Compiler Infrastructure

| Requirement | Description |
|-------------|-------------|
| Tokenizer | Read whitespace-delimited words from input |
| Input source | File reading or stdin |
| Code buffer | Accumulate generated machine code |
| Data segment | Space for variables and data |
| `here` | Current compilation pointer |
| `allot` | Reserve space in data segment |
| `,` | Compile a cell to data segment |
| `c,` | Compile a byte to code/data |
| ELF generation | Output executable binary |

## Layer 6: Self-Hosting Requirements

The compiler must be able to compile every construct it uses in its own source.

| If source uses... | Compiler must handle... |
|-------------------|------------------------|
| String literals | `s"` or equivalent |
| Comments | `\` (line) and `( )` (block) |
| Immediate words | `immediate` and `[` `]` |
| Compile-time execution | `literal` and `postpone` |
| Numeric bases | `hex` `decimal` or `$` prefix |

## Minimal Bootstrap Set

The absolute minimum for a self-hosting compiler:

1. **Define words** - `: ... ;`
2. **Literals** - numbers compile to pushes
3. **Call words** - compile calls to defined words
4. **Basic primitives** - `@` `!` `+` `-` `dup` `drop` `swap`
5. **Conditionals** - `if` `then` `else`
6. **Loops** - at least `begin` `until` or `begin` `while` `repeat`
7. **Output** - write bytes to file

Everything else can be built from these once bootstrap works.

## Test Criteria

For each capability:
- [ ] Compiles without error
- [ ] Generated binary runs
- [ ] Output is correct
- [ ] No memory corruption

## Self-Hosting Test

```bash
# Stage 1: Compile compiler with interpreter
./engine/fifth compiler/X/main.fs compiler/X/main.fs /tmp/stage1

# Stage 2: Compile compiler with stage 1
/tmp/stage1 compiler/X/main.fs compiler/X/main.fs /tmp/stage2

# Stage 3: Compile compiler with stage 2
/tmp/stage2 compiler/X/main.fs compiler/X/main.fs /tmp/stage3

# Verify: stage2 and stage3 must be identical
diff /tmp/stage2 /tmp/stage3 && echo "SELF-HOSTING ACHIEVED"
```

If stage2 != stage3, the compiler is not deterministic or not complete.

# Sixth Compiler

The Sixth compiler (`sixth.fs`) generates x86-64 ELF binaries directly. 1511 lines of Forth emitting machine code bytes. No Rust, no LLVM, no Cranelift.

See [native/README.md](native/README.md) for benchmarks and examples.

## What sixth.fs Actually Compiles

sixth.fs compiles a **subset** of Forth, not Forth.

### Supported

- Integer literals, arithmetic (`+`, `-`, `*`, `/`, `mod`, `negate`, `abs`, `1+`, `1-`, `2*`, `2/`)
- Stack operations (`dup`, `drop`, `swap`, `over`, `rot`, `nip`, `tuck`, `2dup`, `depth`)
- Comparison (`=`, `<>`, `<`, `>`, `0=`, `0<`)
- Logic (`and`, `or`, `xor`, `invert`)
- Control flow (`if`/`else`/`then`, `begin`/`while`/`repeat`, `begin`/`until`)
- Loops (`do`/`loop`/`+loop` with `i`, `j`)
- Word definitions (`: name ... ;`)
- Recursion (by name, not `recurse`)
- Output (`.`, `emit`, `cr`)
- `exit`

### Not Supported

- **Memory** (`variable`, `@`, `!`, `create`, `allot`, `cells`, `here`)
- **Strings** (`s"`, `."`, `type`, `count`)
- **File I/O** (`open-file`, `read-line`, `slurp-file`, `close-file`)
- **System** (`system`, `require`, `include`)
- **Return stack** (`>r`, `r>`, `r@`)
- **Constants** (`constant`, `value`, `to`)

Without `@` and `!` you cannot build anything with state. Without strings you cannot do I/O beyond printing integers. This is an arithmetic compiler, not a language implementation.

## Test Suite Honesty

The test suite has 1050 tests. 999 pass with strict output comparison. That number is real but misleading.

**Every single test is a pure stack-and-arithmetic program.** Zero tests use `variable`, `@`, `!`, `create`, `allot`, strings, file I/O, or any memory operations. The tests were written knowing what sixth.fs can compile. They confirm the supported subset works. They say nothing about whether sixth.fs is a usable Forth compiler.

A real test suite would start from the Forth standard and ask "what can't you compile?" This suite did the opposite.

### Test runner

```bash
# Strict output comparison (Forth runner)
./engine/fifth compiler/tests/run.fs

# Results: PASS/WRONG/CFAIL/RFAIL/SKIP
```

Each test file has a `\ expect: <output>` comment on line 1. The runner compiles with sixth.fs, runs the binary, captures output, and compares against the expected value. Tests without expected output (regression tests) are verified by clean exit only.

## What Would Make This a Real Compiler

1. **Memory access**: `variable`, `@`, `!`, `create`, `allot` — state and data structures
2. **Strings**: `s"`, `type` — real I/O
3. **Return stack**: `>r`, `r>`, `r@` — temporary storage
4. **Constants**: `constant`, `value` — named values
5. **Self-hosting**: sixth.fs should be able to compile sixth.fs

# Sixth Native Compiler Context

You are working on Sixth, a Forth that compiles directly to x86-64 machine code bytes.

## The One True Path

```
Forth source → sixth.fs → raw bytes → ELF binary
```

**NO C. NO RUST. NO CRANELIFT. NO DEPENDENCIES. NO INTERMEDIARIES.**

## What sixth.fs Is

sixth.fs is a **compiler**, not an assembler:
- INPUT: Forth source code (`: fib dup 2 < if ... ;`)
- OUTPUT: raw x86-64 machine code bytes in an ELF binary

It is a compiler because it takes a high-level language (Forth) and outputs the lowest level (bytes). An assembler would take assembly text as input - sixth.fs skips that entirely.

## Architecture Levels

```
Forth       (high-level)     ← sixth.fs INPUT
Assembly    (mov rax, rbx)   ← SKIPPED
Bytes       (0x48 0x89 0xc3) ← sixth.fs OUTPUT (lowest level)
```

## Current State

- **sixth.fs at HEAD** (97 lines): Stripped to byte emission only. The compiler logic was removed.
- **sixth.fs at commit 86490d4** (544 lines): Full compiler with TOS caching. Achieves 40-50% of C speed.
- **Hand-coded benchmarks** in `compiler/native/`: Achieve 90% of C speed.

## Why the Performance Gap

The 544-line compiler uses a memory-based stack (r15 points to memory). Even with TOS cached in rax, operations hit memory.

The hand-coded benchmarks use **registers only** in hot loops:
```asm
ebx = a
ecx = b
r8d = counter
loop: 5 register-only instructions
```

Zero memory access = 90% of C speed.

## The Goal

Build a compiler that generates code like the hand-coded versions:
1. TOS in register (exists at 86490d4)
2. NOS in register (needed)
3. Loop variables in registers (needed)
4. Eliminate memory traffic in hot paths (needed)

## Key Files

| File | Purpose |
|------|---------|
| `compiler/sixth.fs` | The compiler (restore from 86490d4 or rebuild) |
| `compiler/native/*.fs` | Hand-coded benchmarks - the performance target |
| `compiler/native/README.md` | Benchmark results |

## Commands

```bash
# Run a .fs file through the interpreter
./sixth myprogram.fs

# The compiler (when restored) would be:
./sixth compiler/sixth.fs input.fs    # → produces a.out
./a.out                             # → runs native binary
```

## Git Reference

```bash
# See the full compiler with TOS caching:
git show 86490d4:compiler/sixth.fs

# See what was removed:
git diff 86490d4..HEAD -- compiler/sixth.fs
```

## Do NOT

- Mention C codegen
- Mention Cranelift
- Mention Rust
- Mention LLVM
- Suggest any intermediary language
- Call sixth.fs an "assembler"
- Forget that the 544-line compiler existed

## Do

- Work with Forth and bytes only
- Reference the hand-coded benchmarks as the target
- Keep the path direct: Forth → bytes
- Check git history before claiming something doesn't exist

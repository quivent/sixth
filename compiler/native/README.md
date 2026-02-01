# Sixth Native Compiler

**Forth script compiles Forth script to binary machine code.**

No C. No assembly. No linker. No external tools. The Sixth interpreter
runs a Forth program (`sixth.fs`) which reads your Forth source and emits
x86_64 ELF bytes directly. That is all.

```
┌─────────────┐     ┌─────────────┐     ┌─────────────┐
│   sixth     │ --> │  sixth.fs   │ --> │  program    │
│ interpreter │     │  compiler   │     │   (ELF)     │
└─────────────┘     └─────────────┘     └─────────────┘
      C              990 lines Forth      172-500 bytes
```

## Compilers

| File | Lines | Language | Purpose |
|------|-------|----------|---------|
| `../sixth.fs` | 990 | **Forth** | Full native compiler (self-hosting) |
| `../ff.fs` | 346 | **Forth** | Minimal compiler (numbers, arithmetic, print) |

No C in the compilers. Pure Forth. Reads Forth, emits bytes.

## Words Go in the Library

Missing words belong in `lib/core.fs`. Not in every script.

```bash
# Add a word
sixth compiler/addword.fs nip '( a b -- b )' 'swap drop'
```

Scripts that need library words: `require lib/core.fs`

## Goal: No C Anywhere

The C interpreter (`engine/`) is scaffolding. The native compiler eliminates it.

```
Today:    sixth (C) runs sixth.fs → binary
Tomorrow: sixth compiles sixth.fs → sixth (native, no C anywhere)
```

Forth compiling Forth to machine code. No C. No assembler. No linker. That is the goal.

## Philosophy

Write x86_64 machine code byte-by-byte. Emit minimal ELF headers (120 bytes).
The result: standalone executables under 400 bytes that run at native speed.

## Quick Start

```bash
cd compiler/native
../../sixth hello.fs && ./hello        # Hello, World!
../../sixth sum1b.fs && ./sum1b        # Sum 1 to 1 billion
../../sixth sieve-fast.fs && ./sieve-fast  # Count primes to 1M
```

## Benchmarks

### Binary Sizes

| Program | Sixth | C (gcc) | Ratio |
|---------|-------|---------|-------|
| hello | 172 bytes | ~16 KB | **93x smaller** |
| sum1b | 228 bytes | ~16 KB | **70x smaller** |
| loop10b | 178 bytes | ~16 KB | **90x smaller** |
| sieve-fast | 373 bytes | ~16 KB | **43x smaller** |

### Execution Speed

**vs C -O0 (unoptimized):**

| Benchmark | Sixth | C -O0 | Sixth wins by |
|-----------|-------|-------|---------------|
| sum 1B | 0.20s | 0.53s | **2.6x faster** |
| loop 10B | 1.84s | 3.44s | **1.9x faster** |

**vs C -O2 (optimized):**

| Benchmark | Sixth | C -O2 | Notes |
|-----------|-------|-------|-------|
| sieve 1M | 0.002s | 0.003s | **1.5x faster** |
| sum 1B | 0.20s | instant | gcc computes at compile time |
| loop 10B | 1.84s | instant | gcc eliminates the loop |

Sixth generates register-cached loops that beat unoptimized C by 2-3x.
Against -O2, simple loops lose to constant folding, but real algorithms (sieve) run faster.

## Examples

```bash
../../sixth square.fs && ./square        # 49 (7*7)
../../sixth sum.fs && ./sum              # 500000500000 (sum 1-1M)
../../sixth fib.fs && ./fib              # 102334155 (fib 40)
../../sixth fib-rec.fs && ./fib-rec      # 9227465 (recursive fib 35)
../../sixth primes.fs && ./primes        # 78498 (primes to 1M)
../../sixth collatz.fs && ./collatz      # 524 (Collatz steps for 837799)
../../sixth collatz-max.fs && ./collatz-max  # 837799 (longest Collatz under 1M)
../../sixth sieve.fs && ./sieve          # 1229 (primes to 10K)
../../sixth sieve-1m.fs && ./sieve-1m    # 78498 (primes to 1M)
../../sixth sieve-fast.fs && ./sieve-fast   # 78498 (optimized)
../../sixth mandelbrot.fs && ./mandelbrot   # ASCII Mandelbrot
../../sixth qsort.fs && ./qsort          # Sort 16 values
```

## Architecture

- **TOS caching**: Top of stack in rax register
- **Data stack**: Native rsp (no separate Forth stack pointer)
- **Locals**: r12-r15 available
- **Syscalls**: Direct int 0x80 / syscall, no libc
- **Functions**: call/ret for recursion
- **Memory**: mmap for dynamic allocation

## How It Works

Each `.fs` file emits bytes directly:

```forth
\ Emit "mov eax, 42"
$b8 c, 42 d,

\ Emit "syscall"
$0f c, $05 c,
```

The ELF header is 120 bytes. Code follows immediately.
Entry point is 0x400078 (right after the header).

## What's Demonstrated

- **hello.fs**: String data embedded in code
- **fib-rec.fs**: Recursive function calls (call/ret)
- **sieve-*.fs**: mmap syscall, memory-mapped arrays
- **mandelbrot.fs**: Fixed-point arithmetic, nested loops
- **qsort.fs**: Array indexing, sorting algorithm

# Fifth Native Compiler

Generates x86_64 Linux ELF binaries directly from Fifth source.
Zero dependencies - uses raw syscalls, no libc.

## Examples

```bash
cd compiler/native
../../fifth hello.fs && ./hello           # Hello, World!
../../fifth square.fs && ./square         # 49 (7*7)
../../fifth sum.fs && ./sum               # 500000500000 (sum 1-1M)
../../fifth fib.fs && ./fib               # 102334155 (fib 40)
../../fifth fib-rec.fs && ./fib-rec       # 9227465 (recursive fib 35)
../../fifth primes.fs && ./primes         # 78498 (primes to 1M)
../../fifth collatz.fs && ./collatz       # 524 (Collatz steps for 837799)
../../fifth collatz-max.fs && ./collatz-max  # 837799 (longest Collatz under 1M)
../../fifth sieve.fs && ./sieve             # 1229 (primes to 10K via mmap)
../../fifth sieve-1m.fs && ./sieve-1m       # 78498 (primes to 1M via mmap)
../../fifth sieve-fast.fs && ./sieve-fast   # 78498 (SSE2 optimized, 69% of C speed)
../../fifth mandelbrot.fs && ./mandelbrot   # ASCII Mandelbrot set
../../fifth qsort.fs && ./qsort             # Bubble sort 16 values
```

## Performance vs C -O2

| Program | Fifth | C -O2 | Ratio | Binary Size |
|---------|-------|-------|-------|-------------|
| hello | - | - | - | 172 bytes |
| square | 0.34ms | 0.57ms | **1.5x faster** | 222 bytes |
| sum (1M) | 0.50s | 0.52s | **equal** | 227 bytes |
| fib (40) | instant | instant | equal | 227 bytes |
| fib-rec (35) | 27ms | 20ms | 74% | 243 bytes |
| primes (1M) | 41.5s | 41.6s | **equal** | 256 bytes |
| collatz | instant | instant | equal | 242 bytes |
| collatz-max | 118ms | 124ms | **5% faster** | 277 bytes |
| sieve-1m | 1.71ms | 0.72ms | 42% | 333 bytes |
| sieve-fast | 1.04ms | 0.72ms | **69%** | 373 bytes |

## Features Demonstrated

- **hello.fs**: String data, embedded strings in code
- **square.fs**: Basic arithmetic (7*7)
- **sum.fs**: Loops, 64-bit arithmetic
- **fib.fs**: Iterative algorithm
- **fib-rec.fs**: **Recursive function calls** (call/ret)
- **primes.fs**: Nested loops, conditionals, modulo
- **collatz.fs**: Conditionals, 64-bit ops
- **collatz-max.fs**: Complex control flow, tracking maximum
- **sieve.fs/sieve-1m.fs**: **mmap syscall**, dynamic memory allocation
- **sieve-fast.fs**: SSE2 SIMD counting (pcmpeqb, pmovmskb, popcnt)
- **mandelbrot.fs**: Fixed-point arithmetic, nested loops, character output
- **qsort.fs**: Array sorting, mmap, nested loops, indexed memory access

## Architecture

- TOS (top of stack) cached in rax
- Native stack (rsp) for data stack
- r12-r15 for local variables
- Direct syscall interface (no libc)
- call/ret for function calls
- mmap for dynamic memory allocation

## Binary Sizes

All binaries are under 300 bytes. For comparison:
- Smallest "Hello World" in C with libc: ~16KB
- Fifth "Hello World": **172 bytes**

# Sixth vs GCC -O2 Benchmarks

## Summary

|                | Sixth    | GCC -O2  | Winner        |
|----------------|----------|----------|---------------|
| Compile time   | instant  | 30ms     | Sixth         |
| Binary size    | 608-1361 | 16KB     | Sixth (12-26x)|
| Runtime (ack)  | 53ms     | 12ms     | GCC (4.4x)    |
| Runtime (prime)| 4.8ms    | 2.8ms    | GCC (1.7x)    |

## Where GCC Wins

GCC -O2 converts deep recursion into register-cached loops. For `ack(3,10)`:

- GCC keeps 6 recursion levels in registers (rbx, rbp, r12-r15)
- Only makes real calls when recursion exceeds 6 levels
- Most of 3 million iterations become register shuffles

Sixth makes a real call every time. Each call:
1. Push to data stack
2. Push return address
3. Call
4. Pop return
5. Pop data stack

This is 4.4x slower for deep recursion.

## Where Sixth Wins

**Compile time**: Sixth compiles instantly. GCC takes 30ms. For interactive development, this matters.

**Binary size**: Sixth produces 608-byte binaries. GCC produces 16KB. Factor of 26x.

**Simplicity**: The compiler is 2600 lines of Forth. Understandable by one person.

## Raw Data

```
=== COMPILE TIME ===
Sixth ack:     0.00 sec
GCC -O2 ack:   0.03 sec

Sixth primes:  0.00 sec
GCC -O2 primes: 0.03 sec

=== BINARY SIZE ===
ack_sixth:     608 bytes
ack_gcc:       15992 bytes

primes_sixth:  1361 bytes
primes_gcc:    16032 bytes

=== RUNTIME (10 iterations) ===
ack Sixth:     0.53 sec (53ms each)
ack GCC:       0.12 sec (12ms each)

=== RUNTIME (100 iterations) ===
primes Sixth:  0.48 sec (4.8ms each)
primes GCC:    0.28 sec (2.8ms each)
```

## Implications

GCC's recursion-to-loop transformation is a significant optimization that Sixth lacks. For code with deep recursion, expect 2-5x slowdown compared to GCC -O2.

For typical code with loops and shallow call stacks, expect 1.5-2x slowdown.

The tradeoff: instant compilation, tiny binaries, readable compiler.

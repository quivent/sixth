# Implementation Strategy

## The Rule

Every optimization must:

1. Have a test that proves correctness
2. Have a benchmark that shows improvement
3. Not regress any other benchmark by more than 10%
4. Be committed separately with measured delta

If an optimization does not measurably improve at least one benchmark, delete it. Complexity without benefit is a bug.

## Benchmarks

Reference: `compiler/bench/ack.fs` — Ackermann(3,10) = 8189

Compare against GCC -O2. Track ratio over time. Anything under 1.0x means you beat GCC.

## Method

1. Measure before
2. Change one thing
3. Measure after
4. Commit with delta in message

No bash. Benchmarks run in Forth.

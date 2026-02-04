# Benchmark Acceptance Criteria

## Valid Benchmarks

A benchmark is valid if:

1. **Runtime**: Runs between 0.5 and 5 seconds on typical hardware
   - Too fast (<0.5s): Timing noise dominates results
   - Too slow (>5s): Test suite becomes impractical to run

2. **Matching outputs**: The `.fs` and `.c` versions produce identical output
   - Both must have `\ expected:` (Forth) / `// expected:` (C) comments
   - Outputs must match exactly (including whitespace)

3. **Deterministic**: Same output every run, no random behavior

4. **No I/O bottleneck**: Compute-bound, not I/O-bound
   - Printing a single result line is acceptable
   - Printing millions of lines is not a valid compute benchmark

## Statistical Methodology

### Measurement Protocol

1. **Warm-up**: Compile all binaries before timing begins
2. **Runs**: Execute each benchmark 10 times
3. **Metric**: Use the **median** time (robust to outliers)
4. **Outlier handling**: Median naturally ignores extreme values

### Confidence

- With 10 runs, the median provides a stable estimate
- Standard deviation is reported for reference
- Benchmarks with stddev > 10% of median should be investigated

### Environment

- Disable CPU frequency scaling if possible (`cpupower frequency-set -g performance`)
- Close other applications during benchmarking
- Run on consistent hardware for historical comparisons

## Win/Loss/Tie Definitions

For a Sixth/GCC time ratio R where R = (Sixth time) / (GCC time):

| Condition | Classification |
|-----------|----------------|
| R < 1.0 | WIN - Sixth is faster than GCC |
| 0.95 <= R <= 1.05 | TIE - Within 5% of each other |
| R > 1.05 | LOSS - GCC is faster |

**Note**: "Win" means Sixth beat GCC. This is the exceptional case we celebrate.

## Performance Ratio Guidelines

| Ratio | Interpretation | Action |
|-------|----------------|--------|
| 0.8 - 1.0 | Excellent - Sixth matching or beating GCC | Celebrate |
| 1.0 - 1.5 | Good - Competitive with optimized C | Acceptable |
| 1.5 - 2.0 | Acceptable - Room for improvement | Investigate |
| 2.0 - 3.0 | Poor - Likely missing optimization | File issue |
| > 3.0 | Bug - Something is wrong | Urgent fix needed |

## Benchmark Categories

### Expected Performance by Category

| Category | Expected Ratio | Notes |
|----------|----------------|-------|
| Pure arithmetic | 1.0 - 1.2 | Should match GCC closely |
| Memory access | 1.0 - 1.3 | Memory-bound often similar |
| Control flow | 1.0 - 1.5 | Branch prediction matters |
| Deep recursion | 1.0 - 2.0 | Stack discipline overhead |
| String operations | 1.2 - 2.0 | Forth string model differs |
| Complex algorithms | 1.0 - 1.8 | Depends on optimization level |

### Category Definitions

1. **Primitive Isolation**: Tests a single Forth primitive in a tight loop
   - `arith.fs`, `mem.fs`, `ctrl.fs`, `call.fs`, `shift.fs`

2. **Loop Variants**: Tests loop constructs specifically
   - `ploop.fs`, `nested.fs`, `doloop*.fs`, `ploop*.fs`, `whileloop*.fs`

3. **Recursion**: Tests call/return overhead
   - `fib40.fs`, `ack.fs`, `tak.fs`, `rec*.fs`, `tailrec.fs`

4. **Algorithms**: Complete algorithms as integration tests
   - `primes.fs`, `sieve1m.fs`, `matmul.fs`, `quicksort.fs`, etc.

5. **String/Memory**: Byte-oriented operations
   - `string.fs`, `strcat.fs`, `strcmp.fs`, `strrev.fs`

## Success Criteria

The benchmark suite **passes** if:

1. **Win rate > 80%**: More than 80% of benchmarks are wins or ties
2. **No catastrophic failures**: No benchmark has ratio > 3.0x
3. **All benchmarks run**: Every benchmark compiles and produces correct output

The benchmark suite **warns** if:

- Any benchmark has ratio > 2.0x
- Win rate is between 70-80%
- Any benchmark has stddev > 15% of median

The benchmark suite **fails** if:

- Win rate < 70%
- Any benchmark has ratio > 5.0x
- More than 10% of benchmarks fail to compile or produce wrong output

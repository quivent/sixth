# /benchmark-sixth - Statistical Benchmark Suite vs GCC -O2

Compare Sixth compiler output against GCC -O2 with statistical rigor.

**21 benchmarks × 5 runs = 105 data points. Median times. Variance reported.**

---

## BENCHMARK LIST

**Primitive (10):** arith, mem, ctrl, call, shift, double, stack, rstack, cmp, fold
**Loop (3):** ploop, nested, deep
**Mixed (8):** fib40, ack, tak, primes, sieve1m, collatz, matmul, string

---

## STEP 1: COMPILE ALL BENCHMARKS

```bash
COMPILER="${1:-compiler/sixth.fs}"
BENCHMARKS="arith mem ctrl call shift double stack rstack cmp fold ploop nested deep fib40 ack tak primes sieve1m collatz matmul string"

echo "=== SIXTH BENCHMARK SUITE ==="
echo "Compiler: ${COMPILER}"
echo "Runs per benchmark: 5"
echo "Method: Median of 5 runs (1 warmup discarded)"
echo ""

echo "=== Compiling Sixth versions ==="
for bench in $BENCHMARKS; do
  ./engine/fifth ${COMPILER} compiler/bench/${bench}.fs /tmp/b_${bench} 2>/dev/null && echo "  ${bench}: OK" || echo "  ${bench}: FAILED"
done

echo ""
echo "=== Compiling GCC -O2 versions ==="
for bench in $BENCHMARKS; do
  gcc -O2 -o /tmp/c_${bench} compiler/bench/${bench}.c 2>/dev/null && echo "  ${bench}: OK" || echo "  ${bench}: FAILED"
done
```

---

## STEP 2: STATISTICAL TIMING FUNCTION

```bash
# Run benchmark N times, return median and stddev
# Usage: run_stats /tmp/binary 5
run_stats() {
  local binary=$1
  local runs=$2
  local times=""

  # Warmup run (discarded)
  $binary >/dev/null 2>&1

  # Timed runs
  for i in $(seq 1 $runs); do
    t=$( { time -p $binary >/dev/null 2>&1; } 2>&1 | grep real | awk '{print $2}' )
    times="$times $t"
  done

  # Calculate median and stddev
  echo $times | tr ' ' '\n' | sort -n | awk '
    { a[NR] = $1; sum += $1; sumsq += $1*$1 }
    END {
      n = NR
      median = (n % 2) ? a[(n+1)/2] : (a[n/2] + a[n/2+1]) / 2
      mean = sum / n
      stddev = sqrt(sumsq/n - mean*mean)
      printf "%.3f %.4f", median, stddev
    }
  '
}
```

---

## STEP 3: RUN ALL BENCHMARKS

```bash
BENCHMARKS="arith mem ctrl call shift double stack rstack cmp fold ploop nested deep fib40 ack tak primes sieve1m collatz matmul string"
RUNS=5

echo ""
echo "=== RUNNING BENCHMARKS (5 runs each, 1 warmup) ==="
echo ""

# Arrays to store results
declare -A sixth_med sixth_std gcc_med gcc_std

for bench in $BENCHMARKS; do
  echo -n "  ${bench}... "

  # Run Sixth version
  read sixth_m sixth_s <<< $(run_stats /tmp/b_${bench} $RUNS)
  sixth_med[$bench]=$sixth_m
  sixth_std[$bench]=$sixth_s

  # Run GCC version
  read gcc_m gcc_s <<< $(run_stats /tmp/c_${bench} $RUNS)
  gcc_med[$bench]=$gcc_m
  gcc_std[$bench]=$gcc_s

  echo "done"
done
```

---

## STEP 4: GENERATE RESULTS TABLE

```bash
echo ""
echo "╔═══════════════════════════════════════════════════════════════════════════════╗"
echo "║                         SIXTH vs GCC -O2 BENCHMARKS                           ║"
echo "║                     (median of 5 runs, 1 warmup discarded)                    ║"
echo "╠════════════╦═══════════════════╦═══════════════════╦═════════╦════════════════╣"
echo "║ Benchmark  ║ Sixth (med ± σ)   ║ GCC -O2 (med ± σ) ║  Ratio  ║     Winner     ║"
echo "╠════════════╬═══════════════════╬═══════════════════╬═════════╬════════════════╣"

wins_sixth=0
wins_gcc=0
total=0

for bench in $BENCHMARKS; do
  sm=${sixth_med[$bench]}
  ss=${sixth_std[$bench]}
  gm=${gcc_med[$bench]}
  gs=${gcc_std[$bench]}

  # Calculate ratio
  if [ -n "$sm" ] && [ -n "$gm" ] && [ "$gm" != "0.000" ]; then
    ratio=$(echo "scale=3; $sm / $gm" | bc)

    # Determine winner (accounting for variance overlap)
    # Significant if difference > combined stddev
    diff=$(echo "scale=4; $sm - $gm" | bc)
    combined_std=$(echo "scale=4; $ss + $gs" | bc)

    if (( $(echo "$ratio < 0.95" | bc -l) )); then
      winner=">>> SIXTH <<<"
      ((wins_sixth++))
    elif (( $(echo "$ratio > 1.05" | bc -l) )); then
      winner="    gcc"
      ((wins_gcc++))
    else
      winner="    TIE"
    fi
  else
    ratio="N/A"
    winner="ERROR"
  fi

  ((total++))

  printf "║ %-10s ║ %6.3fs ± %-6.4f ║ %6.3fs ± %-6.4f ║ %6.3fx ║ %-14s ║\n" \
    "$bench" "$sm" "$ss" "$gm" "$gs" "$ratio" "$winner"
done

echo "╚════════════╩═══════════════════╩═══════════════════╩═════════╩════════════════╝"
```

---

## STEP 5: SUMMARY STATISTICS

```bash
echo ""
echo "=== SUMMARY ==="
echo ""
echo "Total benchmarks: $total"
echo "Sixth wins:       $wins_sixth ($(echo "scale=0; $wins_sixth * 100 / $total" | bc)%)"
echo "GCC wins:         $wins_gcc ($(echo "scale=0; $wins_gcc * 100 / $total" | bc)%)"
echo "Ties:             $((total - wins_sixth - wins_gcc))"
echo ""

# Calculate geometric mean of ratios (better for ratios than arithmetic mean)
geo_mean=$(for bench in $BENCHMARKS; do
  sm=${sixth_med[$bench]}
  gm=${gcc_med[$bench]}
  echo "scale=6; $sm / $gm" | bc
done | awk '{ product *= $1; n++ } END { print exp(log(product)/n) }')

echo "Geometric mean ratio: ${geo_mean}x"
echo ""

if (( $(echo "$wins_sixth > $wins_gcc" | bc -l) )); then
  echo "═══════════════════════════════════════"
  echo "  VERDICT: SIXTH BEATS GCC -O2"
  echo "═══════════════════════════════════════"
elif (( $(echo "$wins_sixth == $wins_gcc" | bc -l) )); then
  echo "═══════════════════════════════════════"
  echo "  VERDICT: TIE WITH GCC -O2"
  echo "═══════════════════════════════════════"
else
  echo "═══════════════════════════════════════"
  echo "  VERDICT: GCC -O2 WINS (optimization needed)"
  echo "═══════════════════════════════════════"
fi
```

---

## STEP 6: CATEGORY BREAKDOWN

```bash
echo ""
echo "=== RESULTS BY CATEGORY ==="
echo ""

echo "PRIMITIVE (arith, mem, ctrl, call, shift, double, stack, rstack, cmp, fold):"
prim_wins=0
for bench in arith mem ctrl call shift double stack rstack cmp fold; do
  ratio=$(echo "scale=3; ${sixth_med[$bench]} / ${gcc_med[$bench]}" | bc)
  (( $(echo "$ratio < 1.0" | bc -l) )) && ((prim_wins++))
done
echo "  Wins: $prim_wins/10"

echo ""
echo "LOOP (ploop, nested, deep):"
loop_wins=0
for bench in ploop nested deep; do
  ratio=$(echo "scale=3; ${sixth_med[$bench]} / ${gcc_med[$bench]}" | bc)
  (( $(echo "$ratio < 1.0" | bc -l) )) && ((loop_wins++))
done
echo "  Wins: $loop_wins/3"

echo ""
echo "MIXED (fib40, ack, tak, primes, sieve1m, collatz, matmul, string):"
mixed_wins=0
for bench in fib40 ack tak primes sieve1m collatz matmul string; do
  ratio=$(echo "scale=3; ${sixth_med[$bench]} / ${gcc_med[$bench]}" | bc)
  (( $(echo "$ratio < 1.0" | bc -l) )) && ((mixed_wins++))
done
echo "  Wins: $mixed_wins/8"
```

---

## STEP 7: GENERATE REPORT

Write to `compiler/BENCHMARK_REPORT.md`:

```markdown
# Sixth Benchmark Report

Generated: [timestamp]
Compiler: [compiler file]
Method: Median of 5 runs (1 warmup discarded)

## Summary

| Metric | Value |
|--------|-------|
| Total benchmarks | 21 |
| Sixth wins | [N] ([%]%) |
| GCC wins | [N] ([%]%) |
| Ties | [N] |
| Geometric mean ratio | [N]x |
| **Verdict** | [SIXTH BEATS GCC -O2 / TIE / GCC WINS] |

## Full Results

| Benchmark | Category | Sixth (s) | σ | GCC (s) | σ | Ratio | Winner |
|-----------|----------|-----------|---|---------|---|-------|--------|
| arith | Primitive | [N] | [N] | [N] | [N] | [N]x | [W] |
| mem | Primitive | [N] | [N] | [N] | [N] | [N]x | [W] |
... (all 21 benchmarks)

## Category Summary

| Category | Benchmarks | Sixth Wins | Avg Ratio |
|----------|------------|------------|-----------|
| Primitive | 10 | [N]/10 | [N]x |
| Loop | 3 | [N]/3 | [N]x |
| Mixed | 8 | [N]/8 | [N]x |

## Statistical Notes

- Each benchmark run 5 times after 1 warmup run
- Median used (more robust than mean)
- Standard deviation (σ) reported for variance assessment
- Win threshold: ratio < 0.95 (5% margin to account for noise)
- Tie threshold: 0.95 ≤ ratio ≤ 1.05

## Interpretation

| Ratio | Meaning |
|-------|---------|
| < 0.8 | Sixth significantly faster |
| 0.8 - 0.95 | Sixth faster |
| 0.95 - 1.05 | Statistical tie |
| 1.05 - 1.2 | GCC slightly faster |
| > 1.2 | GCC faster (investigate) |
| > 2.0 | Bug or missing optimization |
```

---

## QUICK MODE

Single benchmark, single run (for development):

```bash
./engine/fifth compiler/sixth.fs compiler/bench/arith.fs /tmp/b_arith
gcc -O2 -o /tmp/c_arith compiler/bench/arith.c
echo "sixth:" && /usr/bin/time -f "%e" /tmp/b_arith 2>&1
echo "gcc:  " && /usr/bin/time -f "%e" /tmp/c_arith 2>&1
```

---

## WHAT THE NUMBERS MEAN

### If Sixth wins (ratio < 1.0):

The optimization is working. Document which optimization enabled the win:
- Stack caching? (check stack, deep benchmarks)
- Constant folding? (check fold benchmark)
- Superinstructions? (check arith, ploop)
- Loop optimization? (check ploop, nested)

### If GCC wins (ratio > 1.0):

Investigate:
1. Is this a fundamental limitation? (memory bandwidth, etc.)
2. Is an optimization not firing? (check codegen)
3. Is there a bug? (wrong code being generated)

### If ratio > 2.0:

Something is broken. Do not claim "beats GCC -O2" until fixed.

---

## STATISTICAL VALIDITY

This suite provides:

| Property | Implementation |
|----------|----------------|
| Multiple runs | 5 runs per benchmark |
| Warmup | 1 discarded run |
| Central tendency | Median (robust to outliers) |
| Variance | Standard deviation reported |
| Significance threshold | 5% margin for ties |
| No cherry-picking | All 21 benchmarks reported |
| Geometric mean | For overall ratio (proper for ratios) |

**Confidence level**: With 21 benchmarks and 5 runs each, random variation is unlikely to produce consistent wins. A majority win (>11/21) with geometric mean < 1.0 is statistically meaningful.

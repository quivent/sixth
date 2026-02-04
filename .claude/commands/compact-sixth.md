# /compact-sixth - Surgical Compiler Compaction

Reduce compiler/sixth.fs line count without losing functionality or performance.

**Works on a copy. Original untouched until verification passes.**

---

## STEP 1: CREATE WORKING COPY

```bash
cp compiler/sixth.fs compiler/sixth-compact.fs
```

ALL edits happen to `sixth-compact.fs`. The original `sixth.fs` is untouched.

---

## STEP 2: BASELINE MEASUREMENTS

Record complete baseline of the ORIGINAL:

```bash
# Line count
wc -l compiler/sixth.fs

# Tests
./compiler/tests/test

# Benchmarks (record actual times)
./engine/fifth compiler/sixth.fs compiler/bench/ack.fs /tmp/b_ack
/usr/bin/time -f "%e" /tmp/b_ack 2>&1

./engine/fifth compiler/sixth.fs compiler/bench/fib40.fs /tmp/b_fib
/usr/bin/time -f "%e" /tmp/b_fib 2>&1

./engine/fifth compiler/sixth.fs compiler/bench/primes.fs /tmp/b_primes
/usr/bin/time -f "%e" /tmp/b_primes 2>&1
```

Initialize the running log:
```
=== COMPACTION SESSION ===
Date: [timestamp]

BASELINE (original sixth.fs):
  Lines: [N]
  Tests: [pass] pass, [wrong] wrong
  ack(3,12): [N]s
  fib(40): [N]s
  primes: [N]s
```

---

## STEP 3: SAFE DELETIONS (Zero Risk)

Edit `sixth-compact.fs`. Delete in this order, logging each:

### 3.1 Debug Output
```forth
\ Search for and remove:
." DEF:
." ENTRY:
." LOOKUP:
." COMPILE:
." FOUND:
." NOT FOUND:
```

Log each removal:
```
DELETED: line [N] - debug output "[content]"
```

### 3.2 Dead Code
From AUDIT.md, remove words never called.

Log each:
```
DELETED: [word-name] (lines [N]-[M], [K] lines)
  Reason: Never referenced in codebase
```

### 3.3 Unused Variables
```
DELETED: variable [name] at line [N]
  Reason: Declared but never read
```

### 3.4 Excessive Comments
```
TRIMMED: lines [N]-[M] ([K] lines removed)
  Was: [description of comment block]
  Kept: [what remains]
```

After safe deletions, measure:
```bash
wc -l compiler/sixth-compact.fs
# Swap and test
cp compiler/sixth.fs compiler/sixth.fs.bak && cp compiler/sixth-compact.fs compiler/sixth.fs
./compiler/tests/test
cp compiler/sixth.fs.bak compiler/sixth.fs
```

Log:
```
--- PHASE 1 COMPLETE: Safe Deletions ---
Lines: [before] → [after] (-[N] lines, -[%]%)
Tests: [pass] pass, [wrong] wrong
```

**CHECKPOINT**: Ask user "Safe deletions complete. [N] lines removed. Proceed with factoring?"

---

## STEP 4: FACTORING (Low Risk)

For each factoring, document the transformation:

### 4.1 Flush Pattern
```
FACTORED: flush-swap ct-flush flush-pending
  New word: flush-all ( -- )
  Definition: : flush-all flush-swap ct-flush flush-pending ;
  Instances replaced: [N]
  Lines saved: [M]
```

### 4.2 Stack Depth Save/Restore
```
FACTORED: stack-depth >r ... r> stack-depth!
  New word: with-saved-depth ( xt -- )
  Instances replaced: [N]
  Lines saved: [M]
```

### 4.3 Name Comparison
```
MERGED: dict-name=, fixup-name=, info-name=
  Kept: name= ( addr1 u1 addr2 u2 -- flag )
  Deleted: [list of removed words]
  Lines saved: [M]
```

After each factoring, verify:
```bash
cp compiler/sixth.fs compiler/sixth.fs.bak && cp compiler/sixth-compact.fs compiler/sixth.fs
./compiler/tests/test
cp compiler/sixth.fs.bak compiler/sixth.fs
```

Log:
```
--- PHASE 2 COMPLETE: Factoring ---
Lines: [before] → [after] (-[N] lines, -[%]%)
Tests: [pass] pass, [wrong] wrong
Patterns factored: [N]
```

**CHECKPOINT**: Ask user "Factoring complete. [N] lines removed. Proceed with simplification?"

---

## STEP 5: SIMPLIFICATION (Medium Risk)

Document each merge with before/after:

### 5.1 gen-dot / gen-u. Merge
```
MERGED: gen-dot + gen-u. → gen-print-number
  Before: 2 words, [N] lines total
  After: 1 word, [M] lines
  Difference: Sign handling via flag parameter
  Lines saved: [K]
```

### 5.2 Comparison Generators
```
FACTORED: gen-=, gen-<, gen->, gen-0=, gen-0<, gen-0>
  Common code extracted to: gen-compare-common
  Lines saved: [N]
```

After simplification, verify:
```bash
cp compiler/sixth.fs compiler/sixth.fs.bak && cp compiler/sixth-compact.fs compiler/sixth.fs
./compiler/tests/test
cp compiler/sixth.fs.bak compiler/sixth.fs
```

Log:
```
--- PHASE 3 COMPLETE: Simplification ---
Lines: [before] → [after] (-[N] lines, -[%]%)
Tests: [pass] pass, [wrong] wrong
Words merged: [N]
```

**CHECKPOINT**: Ask user "Simplification complete. [N] lines removed. Proceed with verification?"

---

## STEP 6: FULL VERIFICATION

### 6.1 Line Count
```bash
wc -l compiler/sixth-compact.fs
```

### 6.2 All Tests
```bash
cp compiler/sixth.fs compiler/sixth.fs.bak && cp compiler/sixth-compact.fs compiler/sixth.fs
./compiler/tests/test
cp compiler/sixth.fs.bak compiler/sixth.fs
```

### 6.3 Benchmarks (Critical - Must Not Regress)

Run ALL benchmarks. Compile with compacted compiler, time execution.

```bash
# Compile all benchmarks with COMPACTED compiler
for bench in ack fib40 tak primes sieve1m collatz arith call ctrl double mem shift matmul; do
  ./engine/fifth compiler/sixth-compact.fs compiler/bench/${bench}.fs /tmp/b_${bench} 2>/dev/null
done

# Time each benchmark
echo "=== SIXTH BENCHMARKS ==="
for bench in ack fib40 tak primes collatz arith call ctrl double mem shift; do
  echo -n "${bench}: " && /usr/bin/time -f "%e" /tmp/b_${bench} 2>&1
done
```

### 6.4 Self-Compilation
```bash
# Compact compiler compiles itself
./engine/fifth compiler/sixth-compact.fs compiler/sixth-compact.fs /tmp/sixth2

# Verify the compiled compiler works
cp compiler/sixth.fs compiler/sixth.fs.bak && cp /tmp/sixth2 compiler/sixth.fs
./compiler/tests/test
cp compiler/sixth.fs.bak compiler/sixth.fs
```

### 6.5 GCC -O2 Comparison (The Goal)

Compare ALL benchmarks against GCC -O2.

```bash
# Compile all C versions
for bench in ack fib40 tak primes sieve1m collatz arith call ctrl double mem shift matmul; do
  gcc -O2 -o /tmp/c_${bench} compiler/bench/${bench}.c 2>/dev/null
done

# Compare side by side
echo "=== SIXTH vs GCC -O2 ==="
echo "Benchmark     Sixth    GCC-O2   Ratio"
echo "=========     =====    ======   ====="
for bench in ack fib40 tak primes collatz arith call ctrl double mem shift; do
  sixth_time=$(/usr/bin/time -f "%e" /tmp/b_${bench} 2>&1 | tail -1)
  gcc_time=$(/usr/bin/time -f "%e" /tmp/c_${bench} 2>&1 | tail -1)
  echo "${bench}: sixth=${sixth_time}s gcc=${gcc_time}s"
done
```

Record all times for the report.

---

## STEP 7: GENERATE REPORT

Write to `compiler/COMPACT_REPORT.md`:

```markdown
# Sixth Compiler Compaction Report

Generated: [timestamp]

## Executive Summary

| Metric | Original | Compacted | Change | Status |
|--------|----------|-----------|--------|--------|
| Lines | [N] | [N] | -[N] (-[%]%) | ✓ |
| Tests | [N] pass | [N] pass | - | ✓ |

## GCC -O2 Comparison (All Benchmarks)

**Goal: Beat GCC -O2 (ratio < 1.0x means Sixth is faster)**

| Benchmark | Category | Sixth | GCC -O2 | Ratio | Status |
|-----------|----------|-------|---------|-------|--------|
| ack | Recursive | [N]s | [N]s | [N]x | ✓/✗ |
| fib40 | Recursive | [N]s | [N]s | [N]x | ✓/✗ |
| tak | Recursive | [N]s | [N]s | [N]x | ✓/✗ |
| primes | Loop+mem | [N]s | [N]s | [N]x | ✓/✗ |
| sieve1m | Loop+mem | [N]s | [N]s | [N]x | ✓/✗ |
| collatz | Loop | [N]s | [N]s | [N]x | ✓/✗ |
| arith | Compute | [N]s | [N]s | [N]x | ✓/✗ |
| call | Overhead | [N]s | [N]s | [N]x | ✓/✗ |
| ctrl | Control | [N]s | [N]s | [N]x | ✓/✗ |
| double | Math | [N]s | [N]s | [N]x | ✓/✗ |
| mem | Memory | [N]s | [N]s | [N]x | ✓/✗ |
| shift | Bitwise | [N]s | [N]s | [N]x | ✓/✗ |
| matmul | Combined | [N]s | [N]s | [N]x | ✓/✗ |

### Summary by Category

| Category | Benchmarks | Avg Ratio | Verdict |
|----------|------------|-----------|---------|
| Recursive (ack, fib, tak) | 3 | [N]x | [Win/Lose] |
| Loop (primes, sieve, collatz) | 3 | [N]x | [Win/Lose] |
| Micro (arith, call, ctrl, shift) | 4 | [N]x | [Win/Lose] |
| Memory (mem, double, matmul) | 3 | [N]x | [Win/Lose] |
| **Overall** | **13** | **[N]x** | **[Win/Lose]** |

**Goal: Beat GCC -O2 (ratio < 1.0)**

## Phase 1: Safe Deletions (-[N] lines)

### Debug Output Removed
| Line | Content |
|------|---------|
| [N] | [content] |
...

### Dead Code Removed
| Word | Lines | Reason |
|------|-------|--------|
| [name] | [N] | Never referenced |
...

### Unused Variables Removed
| Variable | Line |
|----------|------|
| [name] | [N] |
...

### Comments Trimmed
| Location | Lines Removed | Description |
|----------|---------------|-------------|
| [N]-[M] | [K] | [what it was] |
...

## Phase 2: Factoring (-[N] lines)

### Patterns Factored
| Pattern | New Word | Instances | Lines Saved |
|---------|----------|-----------|-------------|
| flush-swap ct-flush flush-pending | flush-all | [N] | [M] |
...

### Words Merged
| Original Words | Merged To | Lines Saved |
|----------------|-----------|-------------|
| dict-name=, fixup-name=, info-name= | name= | [N] |
...

## Phase 3: Simplification (-[N] lines)

### Merged Implementations
| Words | Result | Before | After | Saved |
|-------|--------|--------|-------|-------|
| gen-dot, gen-u. | gen-print-number | [N] | [M] | [K] |
...

## Verification Results

### Test Suite
```
TOTAL: [N] PASS: [N] WRONG: 0 SKIP: [N]
```

### Self-Compilation
- Compact compiler compiles itself: ✓
- Compiled compiler passes tests: ✓

### Performance Regression Check
| Benchmark | Original | Compacted | Δ | Status |
|-----------|----------|-----------|---|--------|
| ack | [N]s | [N]s | [+/-]% | ✓/✗ |
| fib40 | [N]s | [N]s | [+/-]% | ✓/✗ |
| primes | [N]s | [N]s | [+/-]% | ✓/✗ |

**Performance regression threshold: <5% slower**

## Untouched (Performance Critical)

These words were NOT modified:
- push-tos, pop-tos (stack caching)
- ct-push, ct-pop, ct-flush (constant folding)
- gen-repeat, gen-1-nzloop (loop optimization)
- last-sets-flags?, cmp-pending (flag elision)
- swap-pending, dup-pending (peephole state)
- dup+, nos+, tuck+ (superinstructions)

## Conclusion

- **Lines reduced**: [N] → [M] (-[K], -[%]%)
- **Tests**: All [N] passing
- **Performance**: [No regression / Improved by X% / Regressed by X%]
- **GCC comparison**: [Beating / Within X% of / Behind by X%]

## Files

- Original preserved: `compiler/sixth.fs`
- Compacted version: `compiler/sixth-compact.fs`
- This report: `compiler/COMPACT_REPORT.md`
```

---

## STEP 8: SWAP (Only After Full Verification)

**CHECKPOINT**: Show the performance summary table. Ask user:

"Compaction complete.
- Lines: [before] → [after] (-[N]%)
- Performance: [status]
- GCC comparison: [status]

Replace original with compacted version?"

If user approves:
```bash
# Backup original
cp compiler/sixth.fs compiler/sixth.fs.pre-compact

# Replace with compacted version
cp compiler/sixth-compact.fs compiler/sixth.fs

# Clean up working copy
rm compiler/sixth-compact.fs

# Commit
git add compiler/sixth.fs compiler/COMPACT_REPORT.md
git commit -m "perf: compact compiler -[N] lines (-[%]%)

Summary:
- Dead code removed: [N] words
- Patterns factored: [N]
- Words merged: [N]

Performance: [unchanged/improved]
All [N] tests pass."
```

If user declines:
```bash
echo "Compacted version preserved at compiler/sixth-compact.fs"
echo "Report at compiler/COMPACT_REPORT.md"
echo "Original unchanged."
```

---

## DO NOT TOUCH

These are performance-critical. Do not modify:

- `push-tos`, `pop-tos` — Stack caching core
- `ct-push`, `ct-pop`, `ct-flush` — Constant folding
- `gen-repeat`, `gen-1-nzloop` — Loop optimization
- `last-sets-flags?`, `cmp-pending` — Flag elision
- `swap-pending`, `dup-pending` — Peephole state
- Superinstruction patterns (`dup+`, `nos+`, `tuck+`)

---

## ABORT

At any checkpoint, user can say "abort":
```bash
rm compiler/sixth-compact.fs
rm compiler/COMPACT_REPORT.md
```

Working copy deleted. Original never touched.

---

## TARGET

| Phase | Target Lines | Cumulative Reduction |
|-------|--------------|---------------------|
| Start | ~3068 | - |
| Safe deletions | ~2900 | -5% |
| Factoring | ~2600 | -15% |
| Simplification | ~2400 | -22% |
| **Stretch goal** | <2000 | -35% |

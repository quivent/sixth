# /integrate-sixth-compaction - Integrate Compacted Compiler

Replace `compiler/sixth.fs` with verified `compiler/sixth-compact.fs`.

**This is a one-way operation.** The original becomes backup; the compact becomes canonical.

---

## PREREQUISITES

1. `compiler/sixth-compact.fs` must exist
2. `compiler/sixth.fs` must exist (the original)

If compact version does not exist:
```
ERROR: compiler/sixth-compact.fs not found.
Run /compact-sixth first to create a compacted version.
```

---

## STEP 1: MEASURE BOTH VERSIONS

```bash
echo "=== SIZE COMPARISON ==="
echo -n "Original:  " && wc -l compiler/sixth.fs
echo -n "Compacted: " && wc -l compiler/sixth-compact.fs
```

Calculate reduction percentage. Display to user:
```
Original:  [N] lines
Compacted: [M] lines
Reduction: [K] lines (-[P]%)
```

---

## STEP 2: VERIFY COMPACTED VERSION

### 2.1 Test Suite

```bash
echo "=== TEST SUITE (compacted) ==="
# Swap and test
cp compiler/sixth.fs compiler/sixth.fs.bak && cp compiler/sixth-compact.fs compiler/sixth.fs
./compiler/tests/test
cp compiler/sixth.fs.bak compiler/sixth.fs
```

**GATE**: If any tests WRONG > 0, STOP:
```
ABORT: Compacted compiler fails [N] tests.
Fix the issues before integration.
```

### 2.2 Self-Compilation

```bash
echo "=== SELF-COMPILATION ==="
./engine/fifth compiler/sixth-compact.fs compiler/sixth-compact.fs /tmp/sixth-selfcomp
```

**GATE**: If compilation fails, STOP:
```
ABORT: Compacted compiler cannot compile itself.
```

### 2.3 Verify Self-Compiled Version

```bash
echo "=== SELF-COMPILED TESTS ==="
cp compiler/sixth.fs compiler/sixth.fs.bak && cp /tmp/sixth-selfcomp compiler/sixth.fs
./compiler/tests/test
cp compiler/sixth.fs.bak compiler/sixth.fs
```

**GATE**: Self-compiled compiler must pass all tests.

### 2.4 Benchmark Comparison

Run key benchmarks with BOTH compilers:

```bash
echo "=== BENCHMARK COMPARISON ==="

# Compile benchmarks with ORIGINAL
./engine/fifth compiler/sixth.fs compiler/bench/fibonacci.fs /tmp/orig_fib 2>/dev/null
./engine/fifth compiler/sixth.fs compiler/bench/ackmed.fs /tmp/orig_ack 2>/dev/null
./engine/fifth compiler/sixth.fs compiler/bench/gcd.fs /tmp/orig_gcd 2>/dev/null

# Compile benchmarks with COMPACTED
./engine/fifth compiler/sixth-compact.fs compiler/bench/fibonacci.fs /tmp/comp_fib 2>/dev/null
./engine/fifth compiler/sixth-compact.fs compiler/bench/ackmed.fs /tmp/comp_ack 2>/dev/null
./engine/fifth compiler/sixth-compact.fs compiler/bench/gcd.fs /tmp/comp_gcd 2>/dev/null

# Time original
echo "Original compiler output:"
echo -n "  fibonacci: " && /usr/bin/time -f "%e" /tmp/orig_fib 2>&1
echo -n "  ackermann: " && /usr/bin/time -f "%e" /tmp/orig_ack 2>&1
echo -n "  gcd:       " && /usr/bin/time -f "%e" /tmp/orig_gcd 2>&1

# Time compacted
echo "Compacted compiler output:"
echo -n "  fibonacci: " && /usr/bin/time -f "%e" /tmp/comp_fib 2>&1
echo -n "  ackermann: " && /usr/bin/time -f "%e" /tmp/comp_ack 2>&1
echo -n "  gcd:       " && /usr/bin/time -f "%e" /tmp/comp_gcd 2>&1
```

**GATE**: If any benchmark regresses by >5%, WARN but allow override:
```
WARNING: [benchmark] regressed by [N]% (orig: [X]s, compact: [Y]s)
This may indicate an optimization was removed.

Continue anyway? (user must confirm)
```

---

## STEP 3: VERIFICATION SUMMARY

Display gate results:

```
=== VERIFICATION SUMMARY ===

Tests:           [PASS/FAIL] ([N] pass, [M] wrong)
Self-compile:    [PASS/FAIL]
Self-comp tests: [PASS/FAIL]
Benchmarks:      [PASS/WARN] (max regression: [N]%)

Size reduction:  [N] -> [M] lines (-[K] lines, -[P]%)
```

**CHECKPOINT**: Ask user:
```
All gates passed. Replace compiler/sixth.fs with compacted version?

This will:
1. Backup original to compiler/sixth.fs.pre-compact
2. Replace sixth.fs with sixth-compact.fs
3. Remove sixth-compact.fs (no longer needed)
4. Stage changes for commit

Proceed? (yes/no)
```

---

## STEP 4: EXECUTE INTEGRATION

If user confirms:

```bash
# Backup original
cp compiler/sixth.fs compiler/sixth.fs.pre-compact

# Replace with compacted version
cp compiler/sixth-compact.fs compiler/sixth.fs

# Remove working copy (now redundant)
rm compiler/sixth-compact.fs

# Final verification
echo "=== FINAL VERIFICATION ==="
./compiler/tests/test
```

---

## STEP 5: UPDATE ENCODING

The compacted compiler may have different structure. Regenerate encoding:

```bash
# Check if ENCODING.md needs update
echo "=== ENCODING CHECK ==="
echo "Review compiler/ENCODING.md for accuracy."
echo "Run /reencode-sixth-compiler if structure changed significantly."
```

---

## STEP 6: COMMIT

**CHECKPOINT**: Ask user:
```
Integration complete. Commit changes?

Files modified:
- compiler/sixth.fs (replaced with compacted version)

Files created:
- compiler/sixth.fs.pre-compact (backup)

Commit message will be:
"perf: integrate compacted compiler (-[N] lines, -[P]%)

All [M] tests pass. Self-compilation verified.
Benchmark performance unchanged.

Backup: compiler/sixth.fs.pre-compact"
```

If user confirms:

```bash
git add compiler/sixth.fs
git commit -m "$(cat <<'EOF'
perf: integrate compacted compiler (-[N] lines, -[P]%)

All [M] tests pass. Self-compilation verified.
Benchmark performance unchanged.

Backup: compiler/sixth.fs.pre-compact

Co-Authored-By: Claude <noreply@anthropic.com>
EOF
)"
```

---

## ROLLBACK

If anything goes wrong after integration:

```bash
cp compiler/sixth.fs.pre-compact compiler/sixth.fs
git checkout compiler/sixth.fs
echo "Rolled back to original compiler."
```

---

## ABORT CONDITIONS

Integration aborts immediately if:

1. `sixth-compact.fs` does not exist
2. Any test fails with compacted compiler
3. Self-compilation fails
4. Self-compiled compiler fails tests
5. User declines at any checkpoint

---

## POST-INTEGRATION CLEANUP

After successful integration and commit, the backup can be removed:

```bash
# Optional: remove backup after confirming commit is good
rm compiler/sixth.fs.pre-compact
```

Only do this after verifying the commit is correct and pushed.

---

## SIGNAL ANALYSIS

From an information-theoretic perspective, this integration:

1. **Preserves signal** - All functionality (tests pass)
2. **Reduces noise** - Dead code, verbose comments removed
3. **Maintains channel capacity** - Benchmark performance unchanged
4. **Increases information density** - Same function, fewer bytes

The compacted compiler should be the new canonical source.

# /sixth-status - Sixth Compiler Project Status

Display comprehensive status of the Sixth native compiler project.

---

## Instructions

Generate a status report with accurate data from these sources:

**Data Collection (run these):**
```bash
wc -l compiler/sixth.fs                           # Line count
./compiler/tests/test                             # Test results
grep -c "^\[x\]" compiler/ROADMAP.md              # Completed items
```

**Reference Files:**
- `compiler/ROADMAP.md` - Checklist (14 items to self-hosting)
- `compiler/AUDIT.md` - Code quality issues
- `compiler/BENCHMARKS.md` - Performance summary
- `BENCHMARK01.md` - Detailed test suite results (100+ tests)
- `compiler/bench/results/*.csv` - Complex benchmark suite (235 tests)

---

## Output Format

```
╔══════════════════════════════════════════════════════════════════╗
║                    SIXTH COMPILER STATUS                         ║
╠══════════════════════════════════════════════════════════════════╣

PROGRESS: XX% complete toward self-hosting sovereignty (X/14 items)

┌─────────────────────────────────────────┐
│ METRICS                                 │
├─────────────────────────────────────────┤
│ Compiler size:     XXXX lines (target: 3500)
│ Tests:             XXXX pass / X wrong / XX skip
│ Words implemented: 115+ of ~130 required
└─────────────────────────────────────────┘

┌─────────────────────────────────────────┐
│ BENCHMARK STATUS                        │
├─────────────────────────────────────────┤
│ Test Suite (compiler/tests/):  ~100 benchmarks
│   Most at 0.88x-1.5x of GCC -O2 (COMPETITIVE)
│
│ Complex Suite (compiler/bench/): 235 benchmarks
│   Passing: 1 (ack only - debug output contamination)
│   sixth-fail: ~48 (missing create/allot patterns)
│   output-fail: ~185 (debug prints in output)
│
│ NOTE: bench/ suite failures are NOT performance
│ problems - they're missing features + debug noise
└─────────────────────────────────────────┘

┌─────────────────────────────────────────┐
│ PERFORMANCE vs GCC -O2 (from test suite)│
├─────────────────────────────────────────┤
│ WORST CASES (Sixth slower):
│   0.53x  fold-before-over   (OVER codegen slow)
│   0.62x  swap               (SWAP codegen slow)
│   0.67x  add                (basic add slow)
│   0.71x  popcount           (bit manipulation)
│   0.72x  fold-deep-chain    (deep constant folding)
│
│ TYPICAL: 0.88x - 1.2x of GCC -O2
│ BEST: 1.5x+ on some folding tests
│
│ DEEP RECURSION (ack): 5.3x slower (19% of GCC)
│   - GCC caches 6 recursion levels in registers
│   - Sixth makes real calls every time
│   - This is the Achilles heel
└─────────────────────────────────────────┘

┌─────────────────────────────────────────┐
│ PHASE STATUS                            │
├─────────────────────────────────────────┤
│ [████████████████] Phase 1-4: Core         DONE
│ [████████░░░░░░░░] Phase 5: Interpreter    PARTIAL
│ [████████████░░░░] Phase 6: File I/O       PARTIAL
│ [░░░░░░░░░░░░░░░░] Phase 7: Delete C       NOT STARTED
│ [░░░░░░░░░░░░░░░░] Phase 8-9: Sovereignty  NOT STARTED
└─────────────────────────────────────────┘

┌─────────────────────────────────────────┐
│ REMAINING WORK (to self-hosting)        │
├─────────────────────────────────────────┤
│ [ ] POSTPONE    - compile compilation
│ [ ] DOES>       - runtime behavior
│ [ ] READ-LINE   - line-by-line reading
│ [ ] INCLUDE     - load and evaluate file
│ ─────── SELF-HOSTING COMPLETE ───────
│ [ ] Delete engine/ - remove C dependency
│ [ ] Test framework - replace bash runner
│ ─────── SOVEREIGNTY COMPLETE ───────
└─────────────────────────────────────────┘

┌─────────────────────────────────────────┐
│ KNOWN ISSUES                            │
├─────────────────────────────────────────┤
│ CODE QUALITY (from AUDIT.md):
│ • compile-builtin: 265 lines (should be ~50)
│ • 3 duplicate whitespace skippers
│ • 3 duplicate name comparers
│ • 2 duplicate stack comment parsers
│ • Debug output contaminating benchmarks
│ • Dead variables: tos-cached, pending-pure
│
│ PERFORMANCE GAPS:
│ • OVER/SWAP codegen needs work (0.5-0.6x)
│ • Deep recursion is 5x slower than GCC
│ • No recursion-to-loop transformation
│
│ BENCHMARK SUITE BROKEN:
│ • Debug prints (." DEF:", etc.) in output
│ • create/allot patterns crash compiler
│ • Fix: remove debug output, add create/allot
└─────────────────────────────────────────┘

┌─────────────────────────────────────────┐
│ SOVEREIGNTY PATH                        │
├─────────────────────────────────────────┤
│ Current:     ~3200 lines, C + bash + Linux
│ After Ph 7:  ~3400 lines, bash + Linux
│ After Ph 9:  ~3550 lines, Linux only
│ After Ph 10: ~3470 lines, NOTHING (bare metal)
└─────────────────────────────────────────┘

NEXT ACTION: Remove debug output from compiler, then
             implement POSTPONE/DOES>/READ-LINE/INCLUDE
```

---

## Key Insights

**Why only 1/235 benchmarks pass:**
The compiler/bench/ suite is NOT failing on performance - it's failing because:
1. Debug output (`." DEF:"`, `." ENTRY:"`, etc.) contaminates stdout
2. Many benchmarks use `create ... allot` patterns that crash the compiler
3. The benchmark harness does strict output comparison

**Actual performance from test suite:**
- Most tests run at 88-120% of GCC -O2 speed
- Only deep recursion (ack) shows the 5x slowdown
- This is competitive, not "20% of C"

**The 5.3x ack slowdown:**
GCC's recursion optimization converts `ack(3,10)` into register-cached loops.
Sixth makes 3+ million real function calls. This is a fundamental architectural
difference, not fixable without major changes.

---

## Accuracy Notes

- Target line count: 3500 (realistic with all features)
- Progress calculation: completed [x] items / 14 total
- Performance data: from BENCHMARK01.md test suite, not broken bench/ suite
- The "20% of C" claim only applies to deep recursion (ack)
- Typical code runs at 88%+ of GCC -O2

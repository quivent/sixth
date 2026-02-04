# /finish-sixth - Sixth Compiler Completion Protocol

Complete the Sixth native compiler. Self-hosting. Beat GCC -O2. No dependencies.

---

## THE GOAL

```
3068 lines → 2000 lines → self-hosting → bare metal
```

**Constraints:**
- All tests must pass (currently 1606)
- Performance must beat GCC -O2 on recursive benchmarks
- Zero external dependencies
- Every line must earn its place

---

## PHASE 1: ORIENTATION

Before any work, run three parallel agents to understand the current state:

### Agent 1: Audit (Chuck Moore)
```
Read compiler/sixth.fs completely. Produce AUDIT.md:
- Size analysis (lines per section)
- Redundancy (duplicate code, repeated patterns)
- Dead code
- Words longer than 10 lines
- Patches and workarounds
- Verdict: minimum achievable size
```

### Agent 2: Encode (Claude Shannon)
```
Read compiler/sixth.fs completely. Produce ENCODING.md:
- 5 data structures (name, purpose, layout)
- 10 state variables
- 5-step compilation flow
- 20 key words with stack effects
- Code generation pattern
- The one trick that makes it work
ONE PAGE MAXIMUM.
```

### Agent 3: Plan (Chuck Moore)
```
Read compiler/sixth.fs completely. Produce COMPACTION_PLAN.md:
- Phase 1: Safe deletions (zero risk)
- Phase 2: Factoring (repeated patterns)
- Phase 3: Simplification (merge similar)
- Phase 4: Structural (larger changes)
- DO NOT TOUCH list (performance critical)
- Verification steps after each phase
```

**Launch all three in parallel using the Task tool.**

---

## PHASE 2: COMPACTION

Execute the compaction plan:

1. **Safe deletions first** — dead code, debug output, excessive comments
2. **Factor repeated patterns** — create helpers, replace incrementally
3. **Simplify** — merge similar words, tighten implementations
4. **Verify after each step:**
   ```bash
   ./compiler/tests/test
   ```

Target: Under 2000 lines.

---

## PHASE 3: SELF-HOSTING

Remaining words needed (from ROADMAP.md):

| Word | Lines | Purpose |
|------|-------|---------|
| FIND | ~20 | Dictionary lookup |
| EXECUTE | ~5 | Call execution token |
| [ ] | ~10 | State switching |
| LITERAL | ~10 | Compile literal |
| INTERPRET | ~30 | Process input buffer |
| EVALUATE | ~40 | Interpret string |
| QUIT | ~30 | Main REPL loop |
| DOES> | ~60 | Set runtime behavior |
| INCLUDE | ~20 | Load files |

~225 lines to self-hosting.

**Verification:**
```bash
./engine/fifth compiler/sixth.fs compiler/sixth.fs /tmp/sixth2
/tmp/sixth2 compiler/sixth.fs compiler/sixth.fs /tmp/sixth3
diff /tmp/sixth2 /tmp/sixth3  # Must be identical
```

---

## PHASE 4: PERFORMANCE

Benchmarks that must beat GCC -O2:

```bash
# Compile and time
./engine/fifth compiler/sixth.fs compiler/bench/ack.fs /tmp/b_ack
gcc -O2 -o /tmp/c_ack compiler/bench/ack.c
/usr/bin/time -f "%e" /tmp/b_ack
/usr/bin/time -f "%e" /tmp/c_ack

# Must test: ack, fib40, primes, tak
```

**DO NOT TOUCH** (performance critical):
- Stack caching (rax=TOS, rbx=NOS, rcx=third)
- Constant folding (ct-push, ct-pop, ct-flush)
- Superinstructions (dup+, nos+, tuck+)
- Loop elimination (1-nzloop, 0=until)
- Flag elision (cmp-pending, last-sets-flags?)
- Swap absorption

---

## KEY FILES

| File | Purpose |
|------|---------|
| `compiler/sixth.fs` | The compiler |
| `compiler/ROADMAP.md` | Self-hosting phases |
| `compiler/AUDIT.md` | Bloat analysis |
| `compiler/ENCODING.md` | One-page architecture |
| `compiler/COMPACTION_PLAN.md` | Reduction strategy |
| `compiler/tests/test` | Test runner (parallel) |
| `compiler/bench/*.fs` | Performance benchmarks |

---

## VERIFICATION COMMANDS

```bash
# Tests (must pass)
./compiler/tests/test

# Benchmarks (must beat gcc -O2)
./engine/fifth compiler/sixth.fs compiler/bench/ack.fs /tmp/b && /tmp/b

# Line count (target: <2000)
wc -l compiler/sixth.fs

# Self-hosting (when ready)
./engine/fifth compiler/sixth.fs compiler/sixth.fs /tmp/sixth2
```

---

## THE LAW

**Sixth depends on nothing.**

No bash. No shell scripts. No C compiler. No Linux (eventually).

When complete: ~3000 lines from power-on to optimized native code.

Chuck Moore solved this in 2001 with colorForth. We are finishing what he started.

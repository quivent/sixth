# Parallel Debug Orchestration Plan

## Confirmed Facts (Phase 1 complete)
- First CF-PUSH NEG occurs compiling `skip-ws` at line 2434
- `parse-stack-comment` correctly parses at source level (ac=4 for str=, ac=0 for skip-ws)
- The bug is a **codegen bug**: compiled sixth produces wrong runtime values
- The compiled compiler's `stack-depth` variable gets value -34 when it should be 0
- This means a compiled word in the compiler produces wrong x86-64 instructions

## The Problem
When sixth compiles itself, the resulting binary has incorrect code for some word(s). The compiled version of `start-def` or `parse-stack-comment` or a word they call produces wrong `stack-depth`/`arg-count` values at runtime.

## Phase 2: 5 Parallel Agents

### Agent 1: Differential Binary Analysis
Compare host vs self-hosted output for a SIMPLE test case.

```bash
# Compile a test with HOST compiler
./engine/fifth compiler/sixth.fs compiler/tests/01-lit.fs /tmp/host-out

# Compile a test with SELF-HOSTED compiler (if it got far enough)
# Or: dump the code-buf state at key points during self-hosting

# Compare: which words produce different code?
```

Approach:
- Add hex dump of code-buf around `start-def`'s compiled code
- Compare what the host generates vs what the self-host generates
- Focus on: `parse-stack-comment`, `start-def`, `skip-ws-only`, `arg-count`, `stack-depth`

### Agent 2: Stack-depth Tracking Bug Hunt
Instrument the host compiler to dump stack-depth changes during self-hosting.

```bash
# In a COPY of sixth.fs, add to the IF handler:
# Before: stack-depth @ cf-push gen-if cf-push
# After:  stack-depth @ dup ." IF-SD=" . cr cf-push gen-if cf-push

# Similarly for gen-call (which modifies stack-depth):
# dup-pending, swap-pending, ct-depth interactions
```

Track every stack-depth mutation during compilation of `skip-ws`.
Find which compiled word produces the wrong stack-depth value.

### Agent 3: gen-call Stack-Depth Bug
The gen-call function (around line 1260-1350) modifies stack-depth based on call-nargs and call-rets.

```forth
call-rets @ call-nargs @ - stack-depth +!
```

If call-nargs or call-rets are wrong for a specific word, stack-depth drifts.
- List every word called by `parse-stack-comment` and `start-def`
- Check if their nargs/rets in info-buf match their actual signatures
- Check if forward references default to wrong nargs

### Agent 4: Forward Reference Analysis
When sixth compiles itself, many words are used before they're defined (forward references).
Forward refs get `call-nargs=1 call-rets=1` by default (line 3097).

If a word with different actual nargs/rets is used as a forward ref,
the stack-depth tracking is wrong.

- List all words used by `start-def`/`parse-stack-comment`/`skip-ws-only`
- Check which ones are defined AFTER their first use in sixth.fs
- Check if scan-all (Pass 1) recorded correct nargs/rets for them
- Find any word where info-buf nargs differs from actual nargs

### Agent 5: Minimal Reproduction
Create the smallest possible input that triggers CF-PUSH NEG.

```forth
: skip-ws ( -- )
  begin
    input-pos @ input-len @ >= if exit then
    input-buf input-pos @ + c@
    dup 32 <= if
      drop 1 input-pos +!
    else
      drop exit
    then
  again ;
```

Does this alone trigger the bug? If not, what's the minimum set of preceding definitions needed?
Binary search the dependencies.

## Phase 3: Fix (after root cause found)
1. Fix on a COPY
2. Run all 1660 tests
3. Run self-hosting
4. Apply to original only if both pass

## Key Hypothesis
The most likely root cause: **forward reference nargs mismatch**.
When sixth compiles itself, words like `skip-ws-only` or `arg-count` are used
before they're defined. Pass 1 (`scan-all`) should record their nargs correctly.
If scan-all fails to parse a stack comment for a key word, it defaults to nargs=1.
Then when that word is called, gen-call adjusts stack-depth by the wrong amount,
causing cumulative drift that reaches -34.

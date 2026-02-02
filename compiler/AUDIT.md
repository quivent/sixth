# AUDIT REPORT: compiler/sixth.fs

**Auditor**: Chuck Moore perspective
**Date**: 2026-02-02
**File**: 3068 lines

## 1. SIZE ANALYSIS

### Total Lines: 3068

### Lines Per Section (Estimated)

| Section | Lines | % |
|---------|-------|---|
| Header comments + documentation | 75 | 2% |
| Buffers/Constants/Variables | 110 | 4% |
| Code emission (c,, d,, q,, ELF) | 35 | 1% |
| Control flow stack | 10 | <1% |
| Dictionary management | 95 | 3% |
| TOS management (push/pop) | 35 | 1% |
| Code gen: stack ops | 120 | 4% |
| Code gen: I/O (space, key, dot, etc.) | 160 | 5% |
| Code gen: arithmetic | 100 | 3% |
| Code gen: comparison | 100 | 3% |
| Code gen: memory | 80 | 3% |
| Code gen: return stack | 35 | 1% |
| Code gen: control flow | 200 | 7% |
| Code gen: do/loop | 100 | 3% |
| Code gen: PNO (<# # #>) | 80 | 3% |
| Code gen: input parsing | 200 | 7% |
| Code gen: interpret/evaluate | 100 | 3% |
| Static string constants | 160 | 5% |
| Tokenizer | 70 | 2% |
| Number parser | 55 | 2% |
| **compile-builtin dispatch** | **265** | **9%** |
| Word info (double-pass) | 95 | 3% |
| Stack comment parsing (x2) | 70 | 2% |
| compile-token/compile-word | 100 | 3% |
| Runtime dict initialization | 60 | 2% |
| Main/file handling | 90 | 3% |

### Which Sections Are Too Big?

1. **compile-builtin**: 265 lines. A giant dispatch table masquerading as a word. 149 string comparisons with nearly identical patterns.

2. **Static string constants**: 160 lines of `s" word" s, 2constant $word`. Pure data that could be generated.

3. **Code gen: input parsing**: 200 lines. Runtime INTERPRET/EVALUATE/PARSE emit inline x86. Complex for what they do.

4. **Code gen: control flow**: 200 lines. Many variations (if, <if, >if, =if, 0<if, 0=if) with redundant patterns.

## 2. REDUNDANCY

### Words That Do Nearly The Same Thing

1. **Three whitespace skippers**:
   - `skip-ws` (line 2168) - used by tokenizer
   - `scan-skip-ws` (line 2621) - used by scanner
   - `skip-ws-only` (line 2757) - used by parse-stack-comment

   All do the same thing. One should suffice.

2. **Three name comparison words**:
   - `fixup-name=` (line 257)
   - `dict-name=` (line 295)
   - `info-name=` (line 2602)

   Identical algorithm. Should be one `name=` word.

3. **Two stack comment parsers**:
   - `scan-stack-comment` (line 2629) - Pass 1, sets scan-nargs/scan-void/scan-io
   - `parse-stack-comment` (line 2765) - Pass 2, sets arg-count/ret-count/is-void

   Nearly identical 34-line words. Different only in which variables they set.

4. **Comparison codegen**:
   - `gen-=`, `gen-<>`, `gen-<`, `gen->`, `gen-<=`, `gen->=` (lines 761-795)

   All call `gen-cmp-setup` then emit 3 different bytes. Could be one word with a parameter.

5. **Special if forms**:
   - `gen-<if`, `gen->if`, `gen-=if` (lines 1127-1152)

   Identical except for one condition byte. Three 8-line words should be one.

### Code Patterns Repeated Instead of Factored

1. **`flush-swap ct-flush flush-pending`** appears 115 times in compile-builtin. This pattern should be factored into one word like `flush-all`.

2. **`2dup $word str= if 2drop ... true exit then`** repeated 149 times. The entire compile-builtin could be a table-driven dispatch.

3. **Stack depth register save/restore** pattern:
   ```forth
   stack-depth @ 2 >= if $53 c, then
   stack-depth @ 3 >= if $51 c, then
   ...
   stack-depth @ 3 >= if $59 c, then
   stack-depth @ 2 >= if $5b c, then
   ```
   Appears in gen-space, gen-spaces, gen-key, gen-dot, gen-u., gen-cr, gen-emit, gen-type, gen-accept. Should be `save-regs` and `restore-regs` words.

### Dead Code

1. **`install-builtins`** (line 2297): Defined but never called. 3 lines of dead code.

2. **`discard-pending`** (line 2305): Defined but never called. 2 lines of dead code.

3. **`tos-cached`** (line 154): Variable declared, initialized, never used.

4. **`pending-pure`** (line 168): Variable declared, mentioned in comment, never used.

5. **`nos+-pending`** (line 675): Variable declared, never used.

6. **`current-def`** (line 2587): Variable declared, never used.

## 3. UNNECESSARY COMPLEXITY

### Abstractions That Cost More Than They Save

1. **Static string interning system** (lines 1990-2005): 2KB buffer, 6 words (`s,`, `str=`, etc.), 149 string constants. All to avoid `s" word"` comparisons at runtime. The interpreter already handles `s"` strings. This machinery exists only because the author didn't trust Forth string literals.

2. **Word info table for double-pass** (lines 2592-2710): 120 lines to track nargs for forward references. The simpler solution: require forward declarations or just default to 1 arg (works 99% of the time).

3. **Compile-time constant stack** (ct-stack): 8-cell stack, 3 words to manage it. Used for constant folding. The optimization is valuable but the machinery is overkill. Could use a single `ct-pending` value.

### Over-Engineering

1. **Loop elimination patterns** in `gen-repeat` (line 1270): Recognizes specific byte patterns for countdown loops and NOS++ loops. Very clever, but adds 30 lines of complexity for an edge case. Chuck would say: let the programmer write better loops.

2. **gen-1-nzloop** (line 1201): 20 lines to recognize `begin swap 1+ swap 1- dup 0> while repeat` and replace with `add rbx,rax; xor eax,eax`. Heroic optimization for a pattern that should be written as a counted loop.

3. **Swap absorption** (optimization 7): Defers swap to see if the next op can absorb it. Adds swap-pending variable and conditionals in 1+, 1-, negate, and all binary ops. Maybe 50 lines of complexity across the file.

### "Clever" Code That Should Be Simple

1. **Fused dup/cmp/while** (optimization 8): Three interacting pending variables (dup-pending, cmp-pending, swap-pending). The flush order matters. Multiple places where you must save values "BEFORE flush-swap clears them." Fragile.

2. **Dictionary flags encoding**: Bits 0-2 for flags, 3-6 for nargs, 7-10 for rets, 11 for inline, 16-23 for word size. Magic numbers everywhere: `$800`, `3 rshift $F and`, `7 rshift $F and`. Should be named constants.

## 4. FACTORING VIOLATIONS

### Words Longer Than 10 Lines

| Word | Lines | Problem |
|------|-------|---------|
| compile-builtin | 265 | Giant dispatch table, not a word |
| gen-dot | 47 | Inline number printing |
| gen-u. | 40 | Same as gen-dot minus sign handling |
| gen-parse | 33 | Inline parser with forward refs |
| scan-stack-comment | 32 | Duplicates parse-stack-comment |
| parse-stack-comment | 34 | Duplicates scan-stack-comment |
| gen-find | 24 | Complex but probably necessary |
| emit-rt-parse | 51 | Could factor into smaller pieces |
| gen-spaces | 27 | Could share code with gen-space |
| gen-repeat | 32 | Loop pattern matching |
| gen-do | 26 | Register shuffling |
| gen-loop | 32 | Trip count optimization |
| gen-+loop | 25 | Same pattern as gen-loop |
| gen-interpret-body | 18 | Acceptable |
| gen-evaluate | 22 | Mostly setup/teardown |
| compile-token | 42 | Could factor lookup from dispatch |
| compile-word | 72 | Too many responsibilities |
| scan-all | 20 | Acceptable |
| compile-file | 54 | Initialization spaghetti |
| emit-dict-entry | 43 | Repetitive qword copies |

### Words Doing More Than One Thing

1. **compile-word**: Handles `:`, `;`, `variable`, `constant`, `create`, `allot`, `,`, `c,`, `]`, `literal`, AND interpret-mode number parsing AND dictionary lookup for constants. Should be at least 3 words.

2. **compile-file**: Loads file, runs scanner, resets state (10 different variables), generates prologue, emits runtime helpers, compiles source, patches start, initializes base variable, initializes source buffer, emits data initializations, initializes runtime dict, calls main, generates epilogue, writes ELF. Should be 5+ words.

3. **gen-loop**: Normal loop AND trip-count optimization AND do-leave patching. Should be factored.

### Missing Helper Words

1. `flush-all` for `flush-swap ct-flush flush-pending`
2. `save-regs` / `restore-regs` for the stack-depth conditional saves
3. `name=` to replace fixup-name=/dict-name=/info-name=
4. `skip-ws` should be shared (one version, not three)
5. `parse-comment` to share between scan and parse stack comment
6. `gen-cmp` with condition-code parameter

## 5. PATCHES

### Code That Looks Like It Was Added Later

1. **Lines 76-78**: `>=`, `<=`, `<>` defined at top because "missing from interpreter." These should be in the interpreter.

2. **Lines 246-250**: Comment about INLINING: DISABLED. The optimization was attempted, found broken, disabled but not removed.

3. **Debug output** (lines 2802, 2950, 2995-3001): `." DEF: "`, `." ENTRY: offset="`, `." BEFORE PROLOGUE:"` etc. Development debugging left in.

4. **gen-rsub** (line 604): "reverse subtract: TOS - NOS (swap sub optimization)" - added to handle swap followed by sub without emitting swap.

### Workarounds

1. **rt-source-addr** (line 136): "pointer to current input buffer" - added because EVALUATE needs to switch input sources, and the original design had a hardcoded buffer address.

2. **do-trip, do-origin, do-sdepth** arrays (lines 170-172): Added for do/loop elimination optimization. The original gen-do was simpler.

3. **fixup-patch, fixup-entry-p** (lines 264-265): Temporary variables to work around stack juggling in add-fixup. Should be refactored.

### TODO Comments or FIXME Markers

None found. Either the code is perfect or the author doesn't believe in marking debt.

### Inconsistent Patterns

1. **Some gen-* words use has-io, some don't**: gen-space, gen-spaces, gen-key, gen-dot, gen-u., gen-cr, gen-emit, gen-type, gen-accept, gen-refill set `1 has-io !`. But gen-find, gen-execute, gen-interpret, gen-evaluate do not.

2. **ct-flush before vs. after flush-swap**: Most handlers do `flush-swap ct-flush`, but some do `flush-cmp` alone, some do `ct-flush` first. The order matters and it's not consistent.

3. **Variable initialization**: Some variables initialized inline (`variable foo 0 foo !`), some after declaration, some not at all.

## 6. VERDICT

### Can This Be 2000 Lines?

Yes, with discipline:
- Replace compile-builtin dispatch (265 lines) with table-driven lookup (~50 lines)
- Eliminate static string machinery (160 lines) - use s" directly
- Merge the three whitespace skippers (save ~20 lines)
- Merge the three name comparers (save ~25 lines)
- Merge the two stack comment parsers (save ~30 lines)
- Factor gen-cmp-* into one word (save ~30 lines)
- Factor special if forms (save ~20 lines)
- Remove dead code (save ~10 lines)
- Factor save-regs/restore-regs (save ~40 lines)
- Remove debug output (save ~5 lines)

**Estimated reduction**: ~650 lines
**Target**: ~2400 lines

### Can This Be 1500 Lines?

Maybe. Would require:
- Removing double-pass forward reference resolution (accept 1-arg default)
- Removing loop elimination optimizations
- Removing swap absorption
- Simplifying dup/cmp/while fusion
- Removing PNO (<# # #>) - rarely used in practice
- Removing some I/O words (spaces, key, accept, refill)

This would lose functionality. The compiler would still work but generate slower code for some patterns.

### What Is The Minimum Size Without Losing Functionality?

**~2000 lines** is achievable while keeping all features. Below that requires sacrificing either:
- Optimization quality
- Word coverage
- Forward reference handling

### What Must Stay Exactly As Is (Performance Critical)?

1. **push-tos / pop-tos / pop-nos** (lines 320-354): The register stack model. Wrong code here = crash.

2. **gen-call** (line 1313): Register save/restore around calls. Must match the stack-depth model.

3. **gen-while-fused / gen-until-fused / gen-repeat** (lines 1237-1301): The loop optimizations. These generate the hot paths.

4. **ct-push / ct-pop / ct-flush** (lines 381-388): Constant folding. Small but essential.

5. **The ELF header** (lines 210-220): Magic numbers that must be exact.

---

## Summary

This compiler is **50% larger than necessary**. The bloat comes from:

1. A 265-line dispatch table that should be data
2. String interning machinery for a problem that doesn't exist
3. Three copies of whitespace skipping
4. Three copies of name comparison
5. Two copies of stack comment parsing
6. Inline debug output
7. Dead variables and words
8. Unextracted common patterns

The optimizations are valuable but the implementation is undisciplined. Chuck would say: **"Simplify. Then simplify again."**

Target: **2000 lines or less**.

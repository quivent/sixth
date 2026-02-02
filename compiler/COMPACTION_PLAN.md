# Compaction Plan: sixth.fs

**Current**: 3068 lines
**Target**: Under 2000 lines
**Required savings**: ~1100 lines

## CONSTRAINTS

- All 1606 tests must still pass
- Performance must match or beat current (targeting GCC -O2)
- All 115+ compiled words must still work
- Stack caching, superinstructions, constant folding must remain

---

## PHASE 1: SAFE DELETIONS

### 1.1 Debug Output Removal (~20 lines)

Lines 2802, 2950-2951, 2995-3001 contain debug output:
```forth
." DEF: " 2dup type ."  code-here=" code-here . cr
." ENTRY: offset=" dup .
." xt=" dup . cr
." BEFORE PROLOGUE: code-here=" code-here . cr
." AFTER PROLOGUE: code-here=" code-here . cr
." AFTER RT-PARSE: code-here=" code-here . cr
." AFTER RT-FIND: code-here=" code-here . cr
```
**Action**: Remove all `."` debug statements in compile-file and emit-dict-entry.

### 1.2 Header Comments (~69 lines)

Lines 1-69 are extensive optimization documentation. Keep only essential notes.
**Action**: Reduce to 15-line summary. Save -54 lines.

### 1.3 Inline Comments (~50 lines scattered)

Many single-line comments explaining obvious x86 can go:
```forth
\ xor eax, eax (depth = 0)
\ mov rax, imm32
```
**Action**: Remove redundant inline comments. Keep stack effect comments.

### 1.4 Dead/Unused Code

- `install-builtins` (lines 2297-2299): Never called, incomplete stub.
- `gen-dup2` (lines 2015, 2321): Uses `$dup2` but standard word is `2dup` - duplicate.

**Action**: Remove unused words.

**Estimated savings: ~130 lines**

---

## PHASE 2: FACTORING

### 2.1 Stack Depth Conditionals (~100 lines)

Pattern appears 15+ times:
```forth
stack-depth @ 3 >= if $51 c, then
stack-depth @ 2 >= if $53 c, then
...
stack-depth @ 3 >= if $59 c, then
stack-depth @ 2 >= if $5b c, then
```

Found in: gen-space, gen-spaces, gen-key, gen-dot, gen-u., gen-cr, gen-emit, gen-accept

**Action**: Create:
```forth
: save-regs ( -- ) stack-depth @ 2 >= if $53 c, then stack-depth @ 3 >= if $51 c, then ;
: restore-regs ( -- ) stack-depth @ 3 >= if $59 c, then stack-depth @ 2 >= if $5b c, then ;
```
Replace 8 instances. Save ~60 lines.

### 2.2 Binary Ops with Constant Folding (~80 lines)

Pattern in compile-builtin for +, -, *, and, or, xor:
```forth
ct-depth @ 2 >= if ct-pop ct-pop OP ct-push
else ct-depth @ 1 = if ct-pop flush-pending gen-OP-imm
else flush-pending gen-OP then then
```

**Action**: Create:
```forth
: fold-binop ( xt-fold xt-imm xt-gen -- )
  ct-depth @ 2 >= if drop drop ct-pop ct-pop execute ct-push
  else ct-depth @ 1 = if nip ct-pop flush-pending execute
  else nip nip flush-pending execute then then ;
```

### 2.3 Comparison Generators (~50 lines)

gen-=, gen-<>, gen-<, gen->, gen-<=, gen->= share pattern:
```forth
gen-cmp-setup
$0f c, XX c, $c0 c,   \ setCC al
$48 c, $0f c, $b6 c, $c0 c,   \ movzx
$48 c, $f7 c, $d8 c,          \ neg
```

**Action**: Create `gen-cmp-finish` helper:
```forth
: gen-cmp-finish ( setcc-byte -- )
  gen-cmp-setup $0f c, c, $c0 c,
  $48 c, $0f c, $b6 c, $c0 c, $48 c, $f7 c, $d8 c, ;
: gen-= ( -- ) $94 gen-cmp-finish ;
: gen-< ( -- ) $9c gen-cmp-finish ;
```
Save ~30 lines.

### 2.4 Scan/Parse Stack Comment (~60 lines)

`scan-stack-comment` (lines 2629-2660) and `parse-stack-comment` (lines 2765-2798) are near-identical.

**Action**: Merge into single parameterized word. Save ~30 lines.

### 2.5 Control Flow IF Variants (~30 lines)

gen-<if, gen->if, gen-=if share pattern with different condition bytes.

**Action**: Parameterize.

### 2.6 Memory Store Pattern (~30 lines)

gen-!, gen-c!, gen-+! share the same register promotion pattern:
```forth
stack-depth @ 3 >= if
  $48 c, $89 c, $c8 c,
  stack-depth @ 4 >= if
    $49 c, $8b c, $0f c,
    $49 c, $83 c, $c7 c, 8 c,
  then
then
-2 stack-depth +!
```

**Action**: Extract to `pop-2`.

**Estimated savings: ~200 lines**

---

## PHASE 3: SIMPLIFICATION

### 3.1 String Constants (~150 lines)

Lines 2014-2163 define 115 string constants like:
```forth
s" dup" s, 2constant $dup
s" drop" s, 2constant $drop
```

**Action**: Use counted strings in a table, iterate with index. Or inline the strings directly in compile-builtin (no separate constant needed for single-use strings).

Alternative: For many rarely-used words, inline the string:
```forth
2dup s" pick" str= if ...
```
This avoids the `$pick` constant. Won't save much per word but adds up.

### 3.2 compile-builtin Dispatch (~300 lines simplified)

The 2000-line if-chain in compile-builtin can be restructured:
- Group related words
- Use jump tables for simple cases
- Reduce repetitive `2dup $X str= if 2drop flush-swap ct-flush ... true exit then` pattern

**Action**: Create dispatch macro pattern:
```forth
: try ( xt 2const -- flag ) 2swap 2over str= if 2drop execute true else 2drop false then ;
```

### 3.3 gen-dot / gen-u. Merge (~40 lines)

These are 90% identical. gen-u. is gen-dot without sign handling.

**Action**: Factor common loop, add flag for signed vs unsigned.

### 3.4 Double-Cell Math (~80 lines)

gen-d+, gen-d- share significant structure. gen-um/mod, gen-sm/rem, gen-fm/mod share setup.

**Action**: Factor shared preambles.

### 3.5 gen-move / gen-fill (~30 lines)

These share the same register promotion epilogue.

**Action**: Factor common epilogue.

**Estimated savings: ~200 lines**

---

## PHASE 4: STRUCTURAL

### 4.1 Runtime Code Emission Consolidation (~50 lines)

emit-rt-find and emit-rt-parse could share more structure for the skip-spaces loop.

### 4.2 Dictionary Operations (~20 lines)

dict-name=, fixup-name=, info-name= are identical implementations.

**Action**: Single `name=` word, parameterized by entry size.

### 4.3 Control Flow Stack (~10 lines)

cf-push/cf-pop could be inlined in most cases - they're trivial.

### 4.4 ELF Header (~10 lines)

elf-header could use array + loop instead of explicit e, calls.

**Estimated savings: ~80 lines**

---

## DO NOT TOUCH

These are performance-critical and must not be changed:

1. **gen-repeat** (lines 1270-1301): Loop elimination logic. Exact byte patterns matter.
2. **gen-while-fused / gen-until-fused** (lines 1237-1257): Peephole condition fusion.
3. **last-sets-flags?** (lines 1227-1231): Flag elision detection.
4. **gen-1-nzloop** (lines 1201-1220): Countdown elimination.
5. **gen-loop** (lines 1370-1401): Do-loop elimination with trip count.
6. **ct-push/ct-pop/ct-flush** (lines 381-388): Constant folding stack.
7. **Literal-op fusion** in compile-builtin: gen-add-imm, gen-mul-imm etc. paths.
8. **Stack caching**: push-tos, pop-tos, pop-nos, stack-depth logic.
9. **Swap absorption** logic in compile-builtin for $swap, $1+, $1-, $negate.
10. **dup-pending / cmp-pending** peephole in $0=, $0>, $0<, $while, $until handlers.

---

## EXECUTION ORDER

### Phase 1 First (Safe)
- No risk of breaking anything
- Immediate line count reduction
- Establishes baseline for further work

### Phase 2 Second (Factoring)
- Create helper words first
- Replace usage incrementally
- Test after each major refactor

### Phase 3 Third (Simplification)
- More invasive changes
- Requires careful testing
- compile-builtin restructure is highest risk

### Phase 4 Last (Structural)
- Depends on earlier phases
- Lowest ROI, highest complexity

---

## VERIFICATION

After each phase:

1. **Run test suite**:
   ```bash
   ./sixth compiler/tests/run.fs
   ```
   All 1606 tests must pass.

2. **Run benchmarks**:
   ```bash
   ./sixth compiler/sixth.fs compiler/bench/ack.fs /tmp/ack && time /tmp/ack
   ./sixth compiler/sixth.fs compiler/bench/fib40.fs /tmp/fib && time /tmp/fib
   ./sixth compiler/sixth.fs compiler/bench/primes.fs /tmp/primes && time /tmp/primes
   ```
   Times must match or beat baseline.

3. **Self-hosting test**:
   ```bash
   ./sixth compiler/sixth.fs compiler/sixth.fs /tmp/sixth2
   /tmp/sixth2 compiler/sixth.fs compiler/sixth.fs /tmp/sixth3
   diff /tmp/sixth2 /tmp/sixth3  # Must be identical
   ```

---

## ESTIMATED TOTALS

| Phase | Lines Saved |
|-------|-------------|
| Phase 1: Safe Deletions | ~130 |
| Phase 2: Factoring | ~200 |
| Phase 3: Simplification | ~200 |
| Phase 4: Structural | ~80 |
| **Total** | **~610** |

**Projected final**: ~2460 lines

To reach 2000, additional aggressive measures needed:
- Inline more strings (eliminate 50+ 2constants)
- Combine gen-dot/gen-u. completely
- More aggressive compile-builtin restructure

**Stretch goal measures** (~500 more lines):
- Replace str= dispatch with hash table or token indices
- Eliminate s, static string mechanism (inline all)
- Merge all comparison generators into single parameterized word
- Aggressive macro-style code generation

---

## RISKS

1. **Forth debugging is hard**: Stack errors cause cryptic crashes
2. **Self-hosting**: Any codegen bug breaks the compiler
3. **x86 byte sequences**: One wrong byte = crash or wrong results
4. **Optimization interactions**: dup-pending + swap-pending + ct-stack interact subtly

## MITIGATION

- Small commits, test after each
- Keep backup of working version
- Use git bisect if regressions appear
- Comment any non-obvious optimization interactions

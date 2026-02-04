# Compaction Integration Guide

The compaction work was done on a stale copy. Here's how to apply the same changes to the current `sixth.fs`.

## Expected Result

~300 lines removed (~10% reduction), all tests still pass.

## Step 1: Add Helper Words

After the definition of `flush-swap`, add:

```forth
: flush-pending ( -- )  pending-call @ ?dup if gen-call then  0 pending-call ! ;
: flush2 ( -- )  flush-swap ct-flush ;
: flush-all ( -- )  flush2 flush-pending ;
```

## Step 2: Remove Unused Variables

Delete these lines (grep to find them):

```forth
variable pending-pure  0 pending-pure !
variable start-depth   0 start-depth !
variable nos+-pending  0 nos+-pending !
variable current-def   0 current-def !
```

**Keep `tos-cached`** - it's being set, may be used.

## Step 3: Remove Dead Code

Delete these words if they still exist:

```forth
: install-builtins ( -- ) ;
: discard-pending ( -- ) ... ;
```

## Step 4: Remove Debug Output

Delete lines matching:

```
." DEF:"
." ENTRY:"
." PATCH-START:"
." START-ADDR:"
." GEN-CALL:"
." PRE-"
." POST-"
." compile-token:"
." DEBUG:"
```

## Step 5: Trim Header

Replace the large header comment block (first ~69 lines) with:

```forth
\ sixth.fs - Forth to x86-64 native compiler
\ Stack: TOS=rax, NOS=rbx, 3rd=rcx, rest=memory at [r15]
\ Registers: r15=data stack, rbp=return stack, r12/r13=DO/LOOP
```

## Step 6: Remove Section Dividers

Delete lines matching: `^\ ----` (section divider comments)

## Step 7: Pattern Replacement

**Be careful not to replace inside the helper definitions themselves.**

After defining the helpers, replace patterns:

```
flush-swap ct-flush flush-pending  →  flush-all
flush-swap ct-flush                →  flush2
```

Use sed carefully:
```bash
# First, mark the helper definitions to protect them
# Then replace patterns
# This is error-prone - manual replacement may be safer
```

## Step 8: Remove Excess Blank Lines

Collapse runs of 3+ blank lines to 2.

## Step 9: Verify

```bash
# Run full test suite
./engine/fifth compiler/tests/run.fs

# Should see: PASS: 1607 (or current count)

# Test benchmarks
./engine/fifth compiler/sixth.fs compiler/bench/fib40.fs /tmp/t && /tmp/t
# Expected: 102334155
```

## Reference

The original compaction attempt is preserved at:
- `compiler/sixth-compact.fs` (stale, missing DOES>/POSTPONE)
- `compiler/COMPACT_REPORT.md` (details of what was removed)

## Quick Wins (if short on time)

Just these give ~200 lines:
1. Trim header (65 lines)
2. Remove section dividers (~30 lines)
3. Remove debug output (~15 lines)
4. Remove unused variables (~8 lines)
5. Collapse blank lines (~80 lines)

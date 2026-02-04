# /clean-sixth-compiler - Remove Audited Bloat

Execute the deletions identified in compiler/AUDIT.md. Safe removals only. Interactive with checkpoints.

---

## PREREQUISITES

1. `compiler/AUDIT.md` must exist (run `/audit-sixth` first)
2. Read the audit completely before proceeding

If AUDIT.md does not exist, stop and tell the user to run `/audit-sixth` first.

---

## STEP 1: BACKUP AND BASELINE

```bash
cp compiler/sixth.fs compiler/sixth.fs.pre-clean
wc -l compiler/sixth.fs
./compiler/tests/test
```

Record baseline in the change log:
```
=== CLEAN SESSION STARTED ===
Date: [timestamp]
Baseline: [lines] lines, [pass] pass, [wrong] wrong
```

**CHECKPOINT**: Ask user "Backup created. Baseline recorded. Proceed with dead code removal?"

---

## STEP 2: DEAD CODE REMOVAL

From AUDIT.md section "Dead Code", process each word:

For EACH word marked deletable:
1. Show the user: "Removing word: [name] at line [N]"
2. Delete the word definition
3. Run regression test:
   ```bash
   ./compiler/tests/test
   ```
4. If WRONG > 0:
   - Restore the word
   - Log: "KEPT: [name] - tests failed without it"
5. If WRONG = 0:
   - Log: "DELETED: [name] - [N] lines removed"

After all dead code processed:
```
--- Dead Code Summary ---
Deleted: [list of words]
Kept: [list of words that couldn't be deleted]
Lines saved: [N]
```

**CHECKPOINT**: Ask user "Dead code pass complete. [N] lines saved. Proceed with debug output removal?"

---

## STEP 3: DEBUG OUTPUT REMOVAL

Search for debug print patterns:
```forth
." DEF:
." ENTRY:
." LOOKUP:
." COMPILE:
." FOUND:
." NOT FOUND:
." DEBUG:
." TRACE:
.s ( when standalone for debugging )
```

For EACH debug statement found:
1. Show: "Found debug output at line [N]: [content]"
2. Remove the line
3. Run regression test
4. Log result

After all debug output processed:
```
--- Debug Output Summary ---
Removed: [N] debug statements
Lines saved: [N]
```

**CHECKPOINT**: Ask user "Debug output removed. Proceed with unused variable removal?"

---

## STEP 4: UNUSED VARIABLE REMOVAL

From AUDIT.md, find variables marked deletable:

For EACH unused variable:
1. Show: "Removing variable: [name]"
2. Delete the variable declaration
3. Search for any references (should be none)
4. Run regression test
5. Log result

```
--- Unused Variables Summary ---
Deleted: [list]
Lines saved: [N]
```

**CHECKPOINT**: Ask user "Variables cleaned. Proceed with comment trimming?"

---

## STEP 5: COMMENT TRIMMING

Identify excessive comments:
- Header blocks longer than 10 lines
- Inline comments stating the obvious
- Commented-out code
- Changelog/history sections

Show user each candidate:
"Found [N]-line comment block at line [M]. Trim to essential? (y/n)"

Keep:
- File purpose (1-2 lines)
- Critical warnings
- Non-obvious explanations

Delete:
- ASCII art banners
- Obvious explanations (`\ increment counter` before `1+`)
- Commented-out old code

```
--- Comment Summary ---
Trimmed: [N] comment blocks
Lines saved: [N]
```

---

## STEP 6: FINAL VERIFICATION

Run full test suite:
```bash
./compiler/tests/test
```

Run benchmarks:
```bash
./engine/fifth compiler/sixth.fs compiler/bench/ack.fs /tmp/b_ack
/usr/bin/time -f "%e seconds" /tmp/b_ack

./engine/fifth compiler/sixth.fs compiler/bench/fib40.fs /tmp/b_fib
/usr/bin/time -f "%e seconds" /tmp/b_fib
```

Compare to baseline. Performance must not regress.

---

## STEP 7: GENERATE REPORT

Write to `compiler/CLEAN_REPORT.md`:

```markdown
# Sixth Compiler Clean Report

## Summary

| Metric | Before | After | Change |
|--------|--------|-------|--------|
| Lines | [N] | [N] | -[N] ([%]%) |
| Tests passing | [N] | [N] | - |
| ack benchmark | [N]s | [N]s | - |
| fib40 benchmark | [N]s | [N]s | - |

## Dead Code Removed

| Word | Lines | Location |
|------|-------|----------|
| [name] | [N] | line [M] |
...

## Dead Code Kept (tests failed)

| Word | Reason |
|------|--------|
| [name] | [why] |
...

## Debug Output Removed

[N] statements removed from lines: [list]

## Unused Variables Removed

[list]

## Comments Trimmed

[N] blocks, [M] lines saved

## Verification

- All [N] tests pass
- Benchmark performance unchanged
- No functional changes
```

---

## STEP 8: CHECKPOINT - COMMIT

**CHECKPOINT**: Ask user "Clean complete. Review CLEAN_REPORT.md. Commit changes?"

If yes:
```bash
git add compiler/sixth.fs compiler/CLEAN_REPORT.md
git commit -m "chore: remove dead code and bloat (-[N] lines)

Removed:
- [summary of deletions]

All tests pass. Benchmarks unchanged."
```

---

## ROLLBACK

At any checkpoint, user can say "rollback":
```bash
cp compiler/sixth.fs.pre-clean compiler/sixth.fs
```

Session aborted, no changes kept.

---

## RULES

1. **Never delete without testing**
2. **Never proceed without checkpoint approval**
3. **Never modify logic** - only remove dead/unused code
4. **Always log every change**
5. **Always verify benchmarks at the end**

# /refresh - Architectural Refresh for Compiler Debugging

Force immediate architectural recalibration during debugging. Updates DEBUG-STATE.md with fresh analysis.

---

## WHEN TO USE

**Automatic triggers** (agent should invoke):
- Entropy stalls (same ±0.1 for 3 experiments)
- Layer transition (bug traced to different layer)
- Every 10 experiments
- After any fix attempt
- On session resume

**Manual trigger** (user invokes):
- When debugging feels stuck
- After context compaction
- To verify architectural understanding
- Before attempting a fix

---

## REFRESH PROTOCOL

### Step 1: Layer Audit

For each layer, determine status:

```
+-------------------------------------------------------------------------------+
| LAYER AUDIT                                                                   |
+-------------------------------------------------------------------------------+
| Layer         | File(s)       | Status | Evidence                             |
|---------------|---------------|--------|--------------------------------------|
| L0 main.fs    | main.fs       | [?]    | [untested / OK / BUG: desc]          |
| L1 asm.fs     | asm.fs        | [?]    | [untested / OK / BUG: desc]          |
| L2 stack.fs   | stack.fs      | [?]    | [untested / OK / BUG: desc]          |
| L3 prims.fs   | prims.fs      | [?]    | [untested / OK / BUG: desc]          |
| L4 opt-*.fs   | opt-fold.fs   | [?]    | [untested / OK / BUG: desc]          |
|               | opt-fuse.fs   | [?]    |                                      |
|               | opt-swap.fs   | [?]    |                                      |
| L5 control.fs | control.fs    | [?]    | [untested / OK / BUG: desc]          |
| L6 defs.fs    | defs.fs       | [?]    | [untested / OK / BUG: desc]          |
| L7 compile.fs | compile.fs    | [?]    | [untested / OK / BUG: desc]          |
| L8 elf.fs     | elf.fs        | [?]    | [untested / OK / BUG: desc]          |
+-------------------------------------------------------------------------------+
```

### Step 2: Width Audit

Check all @ operations on struct fields:

```bash
# Find all @ operations in shannon compiler
grep -n ' @ ' compiler/shannon/*.fs | grep -E '(dict|info|buf)'
```

```
+-------------------------------------------------------------------------------+
| WIDTH AUDIT                                                                   |
+-------------------------------------------------------------------------------+
| Location              | Field Size | Masked?  | Status                        |
|-----------------------|------------|----------|-------------------------------|
| dict-addr @           | 4 bytes    | [Y/N]    | [OK / BUG - needs $FFFFFFFF]  |
| dict-flags @          | 4 bytes    | [Y/N]    | [OK / BUG - needs $FFFFFFFF]  |
| info-buf offset 24    | 1 byte     | [N/A]    | [OK if using c@]              |
| info-buf offset 25    | 1 byte     | [N/A]    | [OK if using c@]              |
| info-buf offset 26-27 | 2 bytes    | [Y/N]    | [check if w@ or @]            |
| info-buf offset 28    | 1 byte     | [N/A]    | [OK if using c@]              |
| info-buf offset 30-33 | 4 bytes    | [Y/N]    | [OK / BUG - needs mask if @]  |
+-------------------------------------------------------------------------------+

REMINDER: Forth @ reads 8 bytes. 4-byte fields MUST be masked with $FFFFFFFF and
```

### Step 3: Pass Verification

Verify each compiler pass independently:

```
+-------------------------------------------------------------------------------+
| PASS VERIFICATION                                                             |
+-------------------------------------------------------------------------------+
| Pass 1 (scan-all):                                                            |
|   Input:  Source tokens                                                       |
|   Output: info-buf populated                                                  |
|   Test:   Add debug prints in scan-all, verify info-buf contents              |
|   Status: [OK / SUSPECT / BUG]                                                |
|   Evidence: [what was checked]                                                |
+-------------------------------------------------------------------------------+
| Pass 2 (compile-all):                                                         |
|   Input:  Source tokens + info-buf                                            |
|   Output: code-buf + dict-buf + ELF                                           |
|   Test:   Disassemble output, compare to expected                             |
|   Status: [OK / SUSPECT / BUG]                                                |
|   Evidence: [what was checked]                                                |
+-------------------------------------------------------------------------------+
```

### Step 4: Codegen Audit

Disassemble the current failing case:

```bash
# Regenerate failing binary
echo '[failing forth code]' > /tmp/t.fs
./engine/fifth compiler/shannon/main.fs /tmp/t.fs /tmp/t_fail

# Disassemble
objdump -D -b binary -m i386:x86-64 /tmp/t_fail

# Also generate working variant for comparison
echo '[working forth code]' > /tmp/t.fs
./engine/fifth compiler/shannon/main.fs /tmp/t.fs /tmp/t_work
objdump -D -b binary -m i386:x86-64 /tmp/t_work
```

```
+-------------------------------------------------------------------------------+
| CODEGEN AUDIT                                                                 |
+-------------------------------------------------------------------------------+
| Source (failing):  [the exact failing code]                                   |
| Source (working):  [the nearest working variant]                              |
| Delta:             [precise difference]                                       |
+-------------------------------------------------------------------------------+
| Expected codegen for failing case:                                            |
|   [offset]: [expected instruction] ; [purpose]                                |
|   [offset]: [expected instruction] ; [purpose]                                |
|   ...                                                                         |
+-------------------------------------------------------------------------------+
| Actual codegen (disassembly):                                                 |
|   [offset]: [actual instruction]                                              |
|   [offset]: [actual instruction]                                              |
|   ...                                                                         |
+-------------------------------------------------------------------------------+
| DIFFERENCES:                                                                  |
|   [offset]: expected [X] got [Y] - [significance]                             |
|   [offset]: MISSING - [what should be there]                                  |
|   [offset]: EXTRA - [what shouldn't be there]                                 |
+-------------------------------------------------------------------------------+
```

### Step 5: Self-Hosting Progress

Attempt self-compilation and record result:

```bash
# Attempt to compile shannon with shannon
./engine/fifth compiler/shannon/main.fs compiler/shannon/main.fs /tmp/shannon2 2>&1
echo "Exit code: $?"
```

```
+-------------------------------------------------------------------------------+
| SELF-HOSTING PROGRESS                                                         |
+-------------------------------------------------------------------------------+
| Last attempt: [timestamp]                                                     |
| Result:       [success / failure]                                             |
| If failure:   [exact error or crash point]                                    |
| Progress:     [===========---------------] [X]%                               |
| Blocks on:    [specific missing feature or bug]                               |
+-------------------------------------------------------------------------------+
| MILESTONES:                                                                   |
|   [X] Compiles minimal programs (42 . cr)                                     |
|   [X] Compiles arithmetic                                                     |
|   [ ] Compiles variables                    <- CURRENT BLOCKER                |
|   [ ] Compiles constants                                                      |
|   [ ] Compiles control flow (if/then/else)                                    |
|   [ ] Compiles loops (begin/while/repeat)                                     |
|   [ ] Compiles user word definitions                                          |
|   [ ] Compiles itself                                                         |
+-------------------------------------------------------------------------------+
```

### Step 6: Hypothesis Recalibration

Based on refresh findings, update probabilities:

```
+-------------------------------------------------------------------------------+
| HYPOTHESIS RECALIBRATION                                                      |
+-------------------------------------------------------------------------------+
| ID | Description                | Before | After | Reason for change          |
|----|----------------------------|--------|-------|----------------------------|
| H1 | [desc]                     | [%]    | [%]   | [evidence from refresh]    |
| H2 | [desc]                     | [%]    | [%]   | [evidence from refresh]    |
| H3 | [desc]                     | [%]    | [%]   | [evidence from refresh]    |
| Hw | Unknown                    | [%]    | [%]   | [new info or ruled out]    |
+-------------------------------------------------------------------------------+
| NEW HYPOTHESES (if any):                                                      |
|   HN: [desc] - suggested by [refresh finding]                                 |
+-------------------------------------------------------------------------------+
| RULED OUT (if any):                                                           |
|   HX: [desc] - disproven by [refresh finding]                                 |
+-------------------------------------------------------------------------------+
```

---

## OUTPUT FORMAT

After completing all steps, emit:

```
+===============================================================================+
| /refresh COMPLETE                                                             |
+===============================================================================+
| LAYER AUDIT:                                                                  |
|   L0 [?]  L1 [?]  L2 [?]  L3 [?]  L4 [?]  L5 [?]  L6 [?]  L7 [?]  L8 [?]     |
|   Legend: [OK] = verified working, [!] = BUG found, [?] = untested            |
+-------------------------------------------------------------------------------+
| WIDTH AUDIT:                                                                  |
|   dict-addr [OK/!]  dict-flags [OK/!]  info-buf [OK/!]                        |
+-------------------------------------------------------------------------------+
| PASS AUDIT:                                                                   |
|   Pass 1 (scan-all):    [OK / SUSPECT / BUG]                                  |
|   Pass 2 (compile-all): [OK / SUSPECT / BUG]                                  |
+-------------------------------------------------------------------------------+
| CODEGEN DELTA:                                                                |
|   [one-line summary of key difference between expected and actual]            |
+-------------------------------------------------------------------------------+
| SELF-HOST: [X]% - blocks on: [feature]                                        |
+-------------------------------------------------------------------------------+
| RECALIBRATED HYPOTHESES:                                                      |
|   H1: [%] (was [%]) - [reason]                                                |
|   H2: [%] (was [%]) - [reason]                                                |
+-------------------------------------------------------------------------------+
| RECOMMENDED NEXT ACTION:                                                      |
|   [specific action based on refresh findings]                                 |
+===============================================================================+
```

---

## UPDATE DEBUG-STATE.md

After refresh, update the state file:

1. **ARCHITECTURE SNAPSHOT** - Update with current layer/width findings
2. **HYPOTHESES** - Update probabilities with recalibrated values
3. **LAST DISASSEMBLY** - Replace with fresh disassembly
4. **SELF-HOSTING PROGRESS** - Update percentage and blocker
5. **METRICS** - Recalculate entropy based on new probabilities
6. **NEXT ACTION** - Set to recommended action from refresh

```bash
# Commit the refresh
git add compiler/shannon/DEBUG-STATE.md
git commit -m "[debug] refresh: [one-line summary of findings]"
```

---

## USAGE

```
/refresh              # Full refresh (all steps)
/refresh layer        # Layer audit only
/refresh width        # Width audit only
/refresh passes       # Pass verification only
/refresh codegen      # Codegen disassembly only
/refresh self-host    # Self-hosting test only
```

---

## QUICK CHECKLIST

```
[ ] Layer audit complete - all layers marked OK/BUG/?
[ ] Width audit complete - all @ on 4-byte fields checked
[ ] Pass 1 verified independently
[ ] Pass 2 verified independently
[ ] Failing case disassembled
[ ] Working case disassembled for comparison
[ ] Self-hosting attempted, result recorded
[ ] Hypotheses recalibrated based on findings
[ ] DEBUG-STATE.md updated
[ ] Changes committed
```

**Refresh is not optional. It prevents drift.**

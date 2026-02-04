# SHANNON COMPILER DEBUG STATE
# Last updated: 2026-02-04
# Session: 50 total experiments (CLOSED)

## QUICK RESUME

Investigation closed - was test artifact, not compiler bug.

All 1660 tests pass. The Shannon compiler is working correctly.

## CURRENT BUG

```
SIGNAL:  echo 'variable x : main 42 x ! ;' > /tmp/t.fs && ./engine/fifth compiler/shannon/main.fs /tmp/t.fs /tmp/t6 && /tmp/t6
         → Binary only 246 bytes, main body missing, crash

WORKING: echo 'variable x : main 42 . cr ;' > /tmp/t.fs && ./engine/fifth compiler/shannon/main.fs /tmp/t.fs /tmp/t5 && /tmp/t5
         → prints 42, works

DELTA:   Using variable x with store operation !
```

**RESOLUTION:** Shell history expansion was converting `!` to `\!` in test files. Compiler was working correctly. When bash's `echo '!'` runs, the `!` gets escaped to `\!`. The Forth compiler saw `\` as comment start, so `! ;` was never parsed. The fix was to use `echo "..."` or `printf` instead, or disable history expansion with `set +H`.

## ARCHITECTURE SNAPSHOT

```
Bug is in: LAYER 7 - compile.fs / main.fs compile-token
Data flow: 42 → ct-push → x → $800 check → ct-push address → ! → compile-! → flush-all → emit-!

Relevant structures:
  dict-buf[24]: code-addr (4 bytes) - MUST mask: $FFFFFFFF and  ✓ DONE
  dict-buf[28]: flags (4 bytes) - MUST mask: $FFFFFFFF and  ✓ DONE
  ct-stack: compile-time stack, ct-push adds, ct-flush emits as literals

Key code path (main.fs:313-319):
  2dup dict-find ?dup if
    dup dict-flags @ $FFFFFFFF and $800 and if
      dict-addr @ $FFFFFFFF and 2 + code-buf + @   \ get imm64 from stub
      ct-push                                       \ push to compile-time stack
      2drop exit                                    \ ← EXITS HERE
    then
    ...
```

## HYPOTHESES

| ID | Description | Prob | Layer | Last Evidence |
|----|-------------|------|-------|---------------|
| H1 | ct-push works but compile-! doesn't flush ct-stack properly | 0% | L7 | RULED OUT - test artifact |
| H2 | After ct-push for x, compilation of ! doesn't happen | 0% | L0 | RULED OUT - test artifact |
| H3 | emit-! codegen itself is broken | 0% | L3 | RULED OUT - test artifact |
| H4 | Test artifact (shell expansion) | 100% | L0 | CONFIRMED - bash `echo '!'` creates `\!` |
| Hw | Unknown | 0% | L? | N/A |

## CERTAIN FACTS (Do not re-verify)

1. dict-flags was reading garbage (8-byte @ on 4-byte field) - FIXED with $FFFFFFFF mask
2. dict-addr same issue - FIXED with $FFFFFFFF mask
3. $800 flag IS set correctly in compile-variable (value $800 stored)
4. Variable declaration alone works
5. Variable reference without store works (`. cr` prints address)
6. The $800 path IS entered (verified with debug prints)
7. Binary /tmp/t6 is only 246 bytes - too small for main body
8. Bug was test artifact - bash `echo '!'` creates `\!`, compiler saw `\` as comment start

## RULED OUT (Do not re-test)

1. ELF generation broken - simple programs work fine
2. dict-buf entry not created - entry exists with correct flag
3. $800 path not entered - debug confirms entry
4. Width bug in flag check - mask applied and verified
5. ALL compiler bugs - issue was shell history expansion escaping `!` to `\!`

## EXPERIMENT LOG (Append-only)

```
[043] 02-04 E:1.80 C:48% | mask dict-flags @ with $FFFFFFFF | still fails | -
[044] 02-04 E:1.65 C:52% | mask dict-addr @ with $FFFFFFFF  | still fails | -
[045] 02-04 E:1.45 C:55% | check binary size | 246 bytes, too small | info
[046] 02-04 E:1.30 C:58% | disassemble /tmp/t6 | main body truncated after `\` | info
[047] 02-04 E:1.10 C:62% | hexdump test file | found `\!` instead of `!` | info
[048] 02-04 E:0.80 C:75% | check bash history expansion | echo '!' → \! | H4 likely
[049] 02-04 E:0.30 C:92% | test with printf instead of echo | works! | H4 confirmed
[050] 02-04 E:0.00 C:100% | run full test suite | 1660 tests pass | CLOSED
```

## BLOCKERS

```
Current: NONE - investigation closed
Type: N/A
```

## GAPS

```
Missing: None - root cause identified
To fill: N/A
```

## LAST DISASSEMBLY

```
Source: variable x : main 42 x ! ;
Binary: /tmp/t6 (246 bytes)

Expected structure:
  0x78-0x87: prologue (save argc/argv, setup stacks, call main)
  0x88-0x92: epilogue (exit syscall)
  0x93-0xa2: variable x stub (mov rax, $800000; ret)
  0xa3-0xXX: main body (push 42, push $800000, store) ← MISSING?

Root cause:
  Test file contained `\!` not `!` due to bash history expansion
  Compiler saw `\` as comment start, stopped parsing
  main body was never compiled because `! ;` was treated as comment
```

## GIT TRAIL

```
470451f 02-04 ......
[earlier] feat(shannon): add $800 inlining for variable/constant/create
[earlier] fix(shannon): mask dict-flags/dict-addr to 32 bits
```

## METRICS

| Metric | Value | Target | Status |
|--------|-------|--------|--------|
| Entropy | 0.00 bits | < 0.5 | [==========] |
| Confidence | 100% | > 80% | [==========] |
| Budget | 11/20 bits | > 0 | [=====-----] |
| Efficiency | 0.73 | > 0.5 | [=======---] |

## SELF-HOSTING PROGRESS

```
[======--------------] 30%
Blocks on: Nothing - variable with store works correctly
Last attempted: Test suite passes
```

## NEXT ACTION

None - investigation closed. All 1660 tests pass.

## SESSION HISTORY

### Session 1 - 2026-02-04 morning
- Started with: Shannon $800 inlining not implemented
- Ended with: $800 flag setting and basic check in place
- Key finding: dict entry fields are 4 bytes but @ reads 8

### Session 2 - 2026-02-04 afternoon
- Started with: Crash on variable usage
- Ended with: Identified mask fix needed for dict-flags and dict-addr
- Key finding: Garbage in high 32 bits caused $800 check to see wrong value

### Session 3 - 2026-02-04 evening
- Started with: Masks applied but still crashes
- Ended with: Disassembly showing truncated main body
- Key finding: Binary too small, main body not being compiled

### Session 4 - 2026-02-04 night (FINAL)
- Started with: Disassembly analysis
- Ended with: Root cause found - TEST ARTIFACT
- Key finding: Bash history expansion converts `echo '!'` to `\!` in output file. The Forth compiler correctly treats `\` as comment start, so `! ;` was never parsed. Compiler was working correctly the entire time. Using `printf` or `set +H` avoids the issue.
- Resolution: All 1660 tests pass. No compiler bug existed.

---
**INVESTIGATION CLOSED: 2026-02-04** - Root cause: shell history expansion artifact, not compiler bug.

---
**SESSION ENDED: 20260204-220532** - Dump saved to `.claude/session-dumps/session-20260204-220532.md`

---
**SESSION ENDED: 20260204-220556** - Dump saved to `.claude/session-dumps/session-20260204-220556.md`

---
**SESSION ENDED: 20260204-220650** - Dump saved to `.claude/session-dumps/session-20260204-220650.md`

---
**SESSION ENDED: 20260204-220657** - Dump saved to `.claude/session-dumps/session-20260204-220657.md`

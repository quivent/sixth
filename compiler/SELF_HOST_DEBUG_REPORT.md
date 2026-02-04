# Self-Hosting Debug Report

## Crash Signature

```
Invalid xt=1491535901589766190 at ip=480 (dict_count=709)
Bytes at ip: 1491535901589766190 4 3 524288 512
CRASH sig=11
```

All 1660 tests pass. Self-hosting only.

## Findings

### 1. Crash Mechanism (fully traced)

The corrupted value `0x14B300000000002E` at ip=480 is a partial overwrite:
- Low 32 bits = `0x2E` = 46 = xt for `=` (correct value)
- High 32 bits = `0x14B30000` = contamination (2 bytes written over bytes 6-7)

ip=480 is the first cell of the `<>` word body (sixth.fs line 8: `: <> = 0= ;`).
It should contain `46` (xt for `=`).

### 2. What Writes the Bad Bytes

Using a `mem_c_store` trap in engine/fifth.h:

```
TRAP c!: addr=486 val=179 (0xB3)  — call chain: gen-then → patch-rel32 → c!
TRAP c!: addr=487 val=20  (0x14)  — same chain
```

`code-buf` is allocated at vm->mem[528]. The writes go to 486-487, which is
**before** code-buf — inside the `<>` body.

### 3. Why gen-then Writes to the Wrong Address

`gen-then` receives `orig = -38` from `cf-pop`, then computes:

```
address = code-buf + orig - 4 = 528 + (-38) - 4 = 486
```

This address lands inside the `<>` definition body instead of inside code-buf.

### 4. Root Cause: stack-depth Goes Negative

The compiler's `stack-depth` variable goes systematically negative during
self-hosting. Observed values: -31, -32, -33, -34, -38, -39, -40, -66, -71,
-72, -99.

For `$if`, the compiler pushes `stack-depth @ cf-push` then `gen-if cf-push`
(code-here). When stack-depth is negative (e.g. -38), that value sits on the
cf-stack. If there is any cf-stack misalignment (push/pop imbalance from a
control structure), gen-then will pop the -38 stack-depth thinking it's a
code-here, and patch-rel32 writes before code-buf.

The stack-depth tracking bug is the root cause. The compiler miscounts stack
effects when compiling complex words, causing stack-depth to drift negative.
This needs investigation into:

- `parse-stack-comment` — does it correctly count nargs for all words?
- `compile-builtin` — do all builtins adjust stack-depth correctly?
- `compile-token` — does the call-nargs/call-rets tracking work for all cases?
- info-buf silent overflow (line 3012: `info-count @ INFO-MAX >= if exit then`)
  drops entries without error, causing missing nargs/rets for later words

### 5. Secondary Issues

| Issue | Location | Status |
|-------|----------|--------|
| vm_run executes after invalid xt detection | engine/vm.c:83 | Still present — causes SIGSEGV |
| 578 definitions > 512 INFO-MAX | sixth.fs:118 | Silently drops entries |
| No bounds check on c, | sixth.fs:135 | Could corrupt code-pos variable |
| cf-push has no overflow check | sixth.fs:179 | Added in current code |

### 6. What Does NOT Fix the Crash

- Bumping DICT-SIZE and INFO-MAX from 512 to 1024 — same crash
- The data-here fix (14608 → 24848) — already applied, not the cause

## Reproduction

```bash
# All tests pass:
./compiler/tests/test

# Self-hosting crashes:
./engine/fifth compiler/sixth.fs compiler/sixth.fs /tmp/out
```

## Next Steps

1. Trace which compiled word first causes stack-depth to go negative
2. Check if parse-stack-comment handles all sixth.fs stack comment formats
3. Check if info-buf overflow (words past entry 512) causes wrong nargs/rets
4. Fix vm_run to abort on invalid xt instead of executing garbage

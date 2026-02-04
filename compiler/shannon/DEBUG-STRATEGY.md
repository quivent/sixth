# Shannon Variable Crash - Debug Strategy

## Status: $800 inlining committed, crash on variable use

**Symptom:** `variable x : main x . cr ;` crashes with `CRASH sig=11, last word: @`
**Working:** `variable x : main 42 . cr ;` compiles and runs correctly

---

## The Correct Model

Trace the channel precisely:

```
1. ./engine/fifth compiler/shannon/main.fs /tmp/v7.fs /tmp/v7
2. Engine loads main.fs
3. main.fs includes: asm.fs, stack.fs, prims.fs, control.fs, rstack.fs,
   io.fs, opt-*.fs, scan.fs, dispatch.fs, elf.fs, defs.fs, strings.fs, compile.fs
4. main.fs ends with: main
5. main calls compile-file with arguments
6. compile-file reads input, runs scan-all, runs compile-all
```

**Key observation:** The crash shows **no debug output** - not even prints at the very start of `main`.

**Paradox:** Module loading works (verified with `-e include`). But the crash appears to happen before `main` even starts.

**Resolution:** This isn't paradoxical once you understand **stdout buffering**.

---

## The Root Cause of Confusion: Buffering

The C runtime buffers stdout. A segfault kills the process **before the buffer flushes**.

The debug prints ARE executing - you just never see them because the crash happens before the buffer is written to the terminal.

---

## The Strategy

### Step 1: Force Output Flushing

Either:
- Use stderr instead of stdout (unbuffered by default)
- Or flush after each print (if the engine supports it)

In the C engine, this might mean:
- Adding `fflush(stdout)` after prints
- Or using `fprintf(stderr, ...)` instead

### Step 2: Isolate the Crash Location

Add prints with explicit flushes. Find the exact word that crashes.

The crash is in one of these paths:
- `scan-all` during Pass 1
- `compile-all` during Pass 2
- Specifically, the `dict-flags @` in the new $800 check

### Step 3: Understand the Memory Access

The crash is `sig=11` (SIGSEGV) on `@` (fetch). Something is fetching from an invalid address.

**Candidates:**
- `dict-flags @` in the new $800 check (main.fs:315)
- `dict-addr @` when reading the stub value (main.fs:317)
- Some other `@` in a code path not yet traced

### Step 4: Fix the Root Cause

Once isolated, the fix will be straightforward - likely:
- An off-by-one error in address calculation
- An uninitialized pointer
- A dict-buf entry that wasn't properly set up

---

## Assessment

**The $800 changes are correct in principle.** The crash is a secondary bug exposed by the new code path.

The strategy:
1. **Don't revert** the $800 changes (they're architecturally correct)
2. **Fix buffering** to see debug output
3. **Isolate** the specific memory access bug
4. **Fix** and test incrementally

---

## The Confusion

The confusion arose from treating the symptom (no output) as the disease.

**The symptom:** No debug output appears
**The disease:** A bad memory access somewhere in the `dict-flags @` path

*Strip away the noise. Find the signal. The bug is a single bad address.*

---

## Next Action

Modify the C engine to flush stdout, or redirect debug output to stderr, then re-run with debug prints to find the exact crash location.

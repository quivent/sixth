# Self-Hosting Crash Fixes - Debug Session Summary

## Root Causes Found and Fixed

### 1. CODE-SIZE Overflow (Line 14)
**Problem:** CODE-SIZE was 262144 (256KB), but compiled code exceeded this.
**Fix:** Increased to 524288 (512KB).
```forth
524288 constant CODE-SIZE   \ 512KB for self-hosting
```

### 2. RT-DICT-MAX Mismatch (Line 63)
**Problem:** RT-DICT-MAX was 256 but DICT-SIZE was 512. Runtime dictionary buffer was too small.
**Fix:** Increased RT-DICT-MAX to 512, updated all dependent addresses:
- rt-dict-buf: 512 * 40 = 20480 bytes
- rt-state: DATA-BASE + 24808
- rt-source-addr: DATA-BASE + 24816
- rt-last-create: DATA-BASE + 24824
- rt-argc: DATA-BASE + 24832
- rt-argv: DATA-BASE + 24840
- rt-slurp-buf: DATA-BASE + 24848
- data-here init: DATA-BASE + 24848 + SLURP-SIZE

### 3. DICT-SIZE Overflow (Line 15)
**Problem:** DICT-SIZE was 512, but compiler has 541+ definitions (251 colon + 57 variable + 23 create + 210 constant).
**Fix:** Increased to 1024.
```forth
1024 constant DICT-SIZE      \ 1024 dictionary entries
```

### 4. Missing /string Builtin
**Problem:** `/string` word was used but not implemented as a builtin.
**Fix:** Added gen-/string (after gen-count around line 796) and compile-builtin handler.
```forth
: gen-/string ( -- )
  \ ( addr u n -- addr+n u-n ) n=rax, u=rbx, addr=rcx
  $48 c, $01 c, $c1 c,     \ add rcx, rax  (addr+n)
  $48 c, $29 c, $c3 c,     \ sub rbx, rax  (u-n)
  $48 c, $89 c, $d8 c,     \ mov rax, rbx  (u-n to TOS)
  $48 c, $89 c, $cb c,     \ mov rbx, rcx  (addr+n to NOS)
  -1 stack-depth +! ;
```
Also added: `s" /string" s, 2constant $/string` and handler in compile-builtin.

### 5. [char] ( and [char] ) Parsing Issues
**Problem:** `[char] (` was being parsed as start of comment during self-hosting.
**Fix:** Replaced all occurrences with ASCII codes:
- `[char] (` → `40`
- `[char] )` → `41`

### 6. Double-WHILE Pattern Not Supported
**Problem:** Pattern `begin...while...while...repeat then` caused CF underflow because WHILE implementation loses first forward reference when second WHILE is processed.
**Fix:** Rewrote all instances to use single WHILE with AND:
```forth
\ Before (broken):
begin c1 while c2 while body repeat then
\ After (works):
begin c1 c2 and while body repeat
```

### 7. scan-all Didn't Track variable/constant/create
**Problem:** scan-all only tracked colon definitions, so variables/constants/creates weren't in info-buf, causing "Unresolved" errors.
**Fix:** Added handlers for variable, constant, and create in scan-all (around line 3050).

### 8. cf-stack Too Small (Line 33)
**Problem:** cf-stack was 64 cells, possibly too small for deep nesting.
**Fix:** Increased to 256 cells.
```forth
create cf-stack 256 cells allot
```

### 9. C Interpreter Stack Sizes
**Problem:** DSTACK_SIZE and RSTACK_SIZE were 256 in engine/fifth.h.
**Fix:** Increased both to 1024 in engine/fifth.h.

### 10. Hardcoded Address in compile-file (Line ~3405)
**Problem:** `DATA-BASE 14608 SLURP-SIZE + +` was hardcoded, didn't match updated rt-slurp-buf offset.
**Fix:** Updated to `DATA-BASE 24848 SLURP-SIZE + +`.

## Debugging Methodology

1. Binary search to find exact crash line
2. Check buffer sizes vs actual usage
3. Trace control flow for stack issues
4. Check for hardcoded values that should be symbolic

## Key Buffer Sizes for Self-Hosting

| Buffer | Size | Notes |
|--------|------|-------|
| CODE-SIZE | 524288 | 512KB for compiled code |
| DICT-SIZE | 1024 | Dictionary entries |
| INPUT-SIZE | 150000 | Source file buffer |
| INFO-MAX | 512 | Info-buf entries |
| RT-DICT-MAX | 512 | Runtime dictionary |
| cf-stack | 256 cells | Control flow stack |
| SLURP-SIZE | 262144 | Runtime file buffer |

# Shannon ARM64 Port Strategy — Expanded

## The Problem

Port a Forth compiler from x86-64/ELF/Linux to ARM64/Mach-O/macOS (Apple Silicon M4 Max).
Every step must produce something testable. No abstraction-only steps.
No debugging spirals. No hypothetical work. Every commit either runs or it doesn't.

---

## GUARDRAILS — READ BEFORE EVERY WORK SESSION

These exist because the last attempt devolved into hours of abstract bug hunting.

**RULE 1: Never write code you can't test within 5 minutes.**
If you can't type a command and see pass/fail, you're doing it wrong. Stop and redesign the step.

**RULE 2: One unknown at a time.**
If asm.fs AND macho.fs AND stack.fs are all new, a failure tells you nothing. Change ONE file, test it, move on.

**RULE 3: Test from the outside in.**
Don't test by reading code. Test by running binaries and checking exit codes or output. The machine doesn't lie.

**RULE 4: If stuck for more than 15 minutes on the same error, stop.**
Write down what you know, what you don't, and what specific byte/instruction you suspect. Then ask for help or diff against the Phase 0 reference binary.

**RULE 5: No refactoring during porting.**
Port first. Optimize later. If it works but emits ugly code, that's fine. Ship it.

**RULE 6: Every phase has a binary that runs.**
If your current phase doesn't end with `./binary; echo $?` printing the expected number, the phase isn't done.

---

## Current State

```
┌─────────────────────────────────────────────────────────┐
│ C Engine (engine/fifth)                                 │
│   Status: WORKS on ARM64 macOS (just rebuilt it)        │
│   Role:   Forth interpreter, runs .fs source files      │
│   Built:  cc → Mach-O ARM64 binary                     │
└─────────────────────────────────────────────────────────┘
        │
        │ loads and interprets
        ▼
┌─────────────────────────────────────────────────────────┐
│ Shannon Compiler (compiler/shannon/*.fs)                │
│   Status: RUNS (it's just Forth source)                 │
│   BUT its OUTPUT is x86-64 ELF → DEAD on this machine  │
└─────────────────────────────────────────────────────────┘
        │
        │ generates
        ▼
┌─────────────────────────────────────────────────────────┐
│ Compiled Output                                         │
│   Current: x86-64 ELF binary → CANNOT RUN on ARM Mac   │
│   Target:  ARM64 Mach-O binary → RUNS natively          │
└─────────────────────────────────────────────────────────┘
```

**The bridge:** The C engine runs on ARM64 macOS. It executes the compiler (Forth source).
The compiler runs fine — it just emits the wrong bytes. We swap the bytes one file at a time.

---

## Architecture Decisions (FINAL — do not revisit during port)

### Register Mapping

```
Role        ARM64 Register   Why
─────────   ──────────────   ───────────────────────────────────
TOS         X19              Callee-saved. Survives SVC calls.
NOS         X20              Callee-saved.
3rd         X21              Callee-saved.
stkptr      X22              Callee-saved. Points to memory overflow stack.
rstack      X28              Callee-saved. Return stack pointer (>r r>).
scratch1    X9               Caller-saved. Free to clobber.
scratch2    X10              Caller-saved. Free to clobber.
```

**Why callee-saved for the stack registers:** On x86, TOS=RAX/NOS=RBX/3rd=RCX are all
caller-saved. Every syscall requires saving and restoring them (push/pop around SVC).
On ARM64, callee-saved X19-X22,X28 survive syscalls automatically. This eliminates
~6 instructions per I/O operation. It's objectively simpler.

**Trade-off:** Syscalls need arguments in X0-X5. We must MOV stack values to X0-X5
before SVC. This is 1-3 MOV instructions. Worth it vs save/restore.

### Memory Addresses

```
CODE-BASE   = 0x100000000    Mach-O standard text base
DATA-BASE   = 0x100100000    Code + 1MB (conservative)
PAGE-SIZE   = 0x4000          16KB pages on Apple Silicon
```

DATA-BASE is where variables, string buffers, and runtime state live.
The 1MB gap between code and data is generous — our code is typically ~50KB.
If code grows beyond 1MB, adjust DATA-BASE. But that won't happen during the port.

### Syscall Convention

```
macOS ARM64 syscalls:
  X16 = syscall number
  X0  = arg1
  X1  = arg2
  X2  = arg3
  X3  = arg4
  SVC #0x80
  Return in X0 (negative = error)

Key syscall numbers:
  exit  = 1    (X0 = exit code)
  write = 4    (X0 = fd, X1 = buf, X2 = len)
  read  = 3    (X0 = fd, X1 = buf, X2 = len)
  open  = 5    (X0 = path, X1 = flags, X2 = mode)
  close = 6    (X0 = fd)
```

### Binary Format

Mach-O with LC_MAIN + dynamic linking. macOS ARM64 does NOT support static
LC_UNIXTHREAD binaries — the kernel refuses to execute them. We MUST link
through dyld even though our code uses SVC #0x80 directly.

Required load commands:
- LC_SEGMENT_64 __PAGEZERO, __TEXT, __LINKEDIT
- LC_LOAD_DYLINKER ("/usr/lib/dyld")
- LC_BUILD_VERSION (platform=macOS)
- LC_MAIN (entry offset, not absolute address)
- LC_LOAD_DYLIB ("/usr/lib/libSystem.B.dylib")
- LC_SYMTAB, LC_DYSYMTAB
- LC_DYLD_CHAINED_FIXUPS (required by codesign, even if empty)
- LC_DYLD_EXPORTS_TRIE (exports __mh_execute_header and _main)

Leave 16 bytes of padding after load commands for codesign to insert LC_CODE_SIGNATURE.
Do NOT pre-allocate LC_CODE_SIGNATURE — codesign rejects zero-filled signature data.

LINKEDIT must contain: chained fixups data, exports trie, nlist entries, string table.

### Code Signing

macOS requires all ARM64 executables to be signed. Ad-hoc signing:
```bash
codesign -f -s - /tmp/binary
```
The `-f` (force) flag is required — without it, codesign may refuse to replace an existing
signature. This must happen after every binary is written, before it can be executed.

codesign inserts its own LC_CODE_SIGNATURE load command into the padding space and
appends the signature data after the LINKEDIT content.

---

## What Changes and What Doesn't — Complete Inventory

### Files That Get Rewritten (arch/x86/ → arch/arm64/)

| File | Lines | What It Does | ARM64 Equivalent |
|------|-------|-------------|-----------------|
| `asm.fs` | 380 | x86 instruction encoding | ARM64 instruction encoding |
| `elf.fs` | 94 | ELF64 header generation | Mach-O header generation (→ macho.fs) |
| `stack.fs` | 174 | Register-mapped stack (RAX/RBX/RCX/R15) | Same abstraction (X19/X20/X21/X22) |
| `prims.fs` | 502 | 50+ codegen words (emit-add, emit-@, etc.) | Same words, ARM64 instructions |
| `control.fs` | 239 | if/then/else, do/loop, branch patching | Same words, ARM64 branch encoding |
| `rstack.fs` | 55 | >r, r>, r@ using RBP | Same words using X28 |
| `io.fs` | 289 | Linux syscalls via `$0f c, $05 c,` | macOS syscalls via SVC #0x80 |

### Files That Stay The Same (no changes needed)

| File | Lines | Why |
|------|-------|-----|
| `scan.fs` | ~200 | Pure text parsing, no codegen |
| `dispatch.fs` | ~150 | Just a name lookup table |
| `opt-fold.fs` | ~100 | Pure arithmetic on compile-time stack |
| `opt-fuse.fs` | ~80 | Decision logic only, delegates to prims |
| `opt-swap.fs` | ~40 | Boolean flag tracking only |

### Files With Inline x86 Bytes That Must Change

These files are in the "arch-independent" directory but have leaked x86 bytes.
Each inline byte sequence is listed with its exact ARM64 replacement.

**defs.fs** — Variable/constant/create stubs:
```
LINE  x86 BYTES                    MEANING              ARM64 REPLACEMENT
48    $48 c, $b8 c, ... q,        mov rax, imm64       MOVZ/MOVK X9, imm64 (1-4 insns)
49    $c3 c,                       ret                  MOV X19, X9; RET (2 insns)
27-34 $53/$51/$e8/$59/$5b c,       push/pop/call        STP/LDP X20,X21 + BL
```
The $800 stub pattern (mov rax,imm64; ret) becomes: load imm64 into X9 via MOVZ/MOVK,
then MOV X19, X9; RET. The `$800` inlining reads the imm64 from the stub — this offset
changes from +2 (after `$48 $b8`) to wherever the MOVZ immediate bits are. **This needs
a different inlining strategy for ARM64** — either store the value in a known location
in the stub, or always inline the value at compile time without reading it back from code.

**strings.fs** — String literal embedding:
```
LINE  x86 BYTES                    MEANING              ARM64 REPLACEMENT
17    $e9 c, ... 0 d,             jmp rel32            B imm26 (ARM64 unconditional branch)
37    $48 c, $b8 c, swap q,       mov rax, addr        MOVZ/MOVK X19, addr
39    $48 c, $b8 c, q,            mov rax, len         push-val; MOVZ/MOVK X19, len
```
The jmp-over-string pattern changes from 5-byte `$e9 + rel32` to 4-byte `B imm26`.
The displacement calculation changes: x86 is byte-relative from after the instruction,
ARM64 is instruction-relative from the instruction itself.

**compile.fs** — Optimization bypass for NOS operations:
```
LINE  x86 BYTES                    MEANING              ARM64 REPLACEMENT
224   $48 c, $f7 c, $db c,        neg rbx (NOS)        NEG X20, X20 (1 insn, 4 bytes)
233   $48 c, $ff c, $c3 c,        inc rbx (NOS)        ADD X20, X20, #1 (1 insn, 4 bytes)
243   $48 c, $ff c, $cb c,        dec rbx (NOS)        SUB X20, X20, #1 (1 insn, 4 bytes)
260   1 tos sar-ri                 sar rax, 1           1 tos asr-ri (uses ARM64 asm.fs)
661   $ff c, $d0 c,               call rax (execute)   BLR X19 (1 insn, 4 bytes)
```

**main.fs** — Prologue/epilogue + call generation:
```
LINE  x86 BYTES                    MEANING
274-282  gen-prologue              argc/argv capture, stack setup, call main
294-297  gen-epilogue              exit(0) syscall
337-343  compile-token calls       push rbx/rcx, call rel32, pop rcx/rbx
```
All of gen-prologue and gen-epilogue are rewritten for ARM64/macOS.
The compile-token call sequence becomes: STP X20,X21,[SP,#-16]!; BL offset; LDP X20,X21,[SP],#16.

---

## Phase 0: Reference Binaries (No Compiler Changes)

**Goal:** Prove we can create and run ARM64 Mach-O binaries on this machine.

### Step 0.1: Reference Binary Generator

The reference binary generator is `tools/macho-test.c` (~440 lines). It produces
dynamically-linked Mach-O ARM64 executables using LC_MAIN + dyld.

**Key implementation details discovered during Phase 0:**

1. **11 load commands** (NCMDS=11, SIZEOFCMDS=576):
   - LC_SEGMENT_64 __PAGEZERO (72 bytes)
   - LC_SEGMENT_64 __TEXT + __text section (152 bytes)
   - LC_SEGMENT_64 __LINKEDIT (72 bytes)
   - LC_LOAD_DYLINKER "/usr/lib/dyld" (32 bytes)
   - LC_BUILD_VERSION platform=macOS (32 bytes)
   - LC_MAIN entryoff=CODE_OFFSET (24 bytes)
   - LC_LOAD_DYLIB "/usr/lib/libSystem.B.dylib" (56 bytes)
   - LC_SYMTAB (24 bytes)
   - LC_DYSYMTAB (80 bytes)
   - LC_DYLD_CHAINED_FIXUPS (16 bytes)
   - LC_DYLD_EXPORTS_TRIE (16 bytes)

2. **16 bytes of padding** after load commands for codesign to insert LC_CODE_SIGNATURE.
   CODE_OFFSET = 32 (header) + 576 (cmds) + 16 (pad) = 624.

3. **LINKEDIT section** at file offset PAGE_SIZE (0x4000):
   - Chained fixups: 56 bytes (empty, but required by codesign)
   - Exports trie: 48 bytes (exports `__mh_execute_header` and `_main`)
   - Symbol table: 32 bytes (2 nlist_64 entries)
   - String table: 40 bytes
   - Padding: 336 bytes (for codesign to append signature data)
   - Total: 512 bytes

4. **Exports trie** is hand-encoded with ULEB128 offsets:
   - Root → edge "_" → child with 2 edges
   - "_mh_execute_header" → terminal, flags=0, addr=0
   - "main" → terminal, flags=0, addr=624 (ULEB128: 0xF0, 0x04)

5. **codesign behavior**: With NCMDS=11, codesign finds 16 bytes of free space,
   inserts LC_CODE_SIGNATURE there, updates NCMDS→12 and SIZEOFCMDS→592,
   appends signature data after LINKEDIT.

See `tools/macho-test.c` for the complete implementation.

### Step 0.2: Build and Test

```bash
# Build the tool
cc -o tools/macho-test tools/macho-test.c

# Test 1: exit(42)
./tools/macho-test exit 42 /tmp/test42
chmod +x /tmp/test42 && codesign -f -s - /tmp/test42
/tmp/test42; echo $?
# EXPECTED: 42

# Test 2: arithmetic (6*7=42)
./tools/macho-test arith 0 /tmp/test-arith
chmod +x /tmp/test-arith && codesign -f -s - /tmp/test-arith
/tmp/test-arith; echo $?
# EXPECTED: 42

# Test 3: hello world
./tools/macho-test hello 0 /tmp/test-hello
chmod +x /tmp/test-hello && codesign -f -s - /tmp/test-hello
/tmp/test-hello
# EXPECTED: Hello
# (with newline)

# Inspect headers
otool -l /tmp/test42
# LOOK FOR: LC_MAIN with entryoff = 624 (0x270)
# LOOK FOR: __TEXT segment with vmaddr = 0x100000000
# LOOK FOR: __LINKEDIT segment
# LOOK FOR: LC_DYLD_CHAINED_FIXUPS and LC_DYLD_EXPORTS_TRIE

# Verify exports
dyld_info -exports /tmp/test42
# SHOULD SHOW: _main and __mh_execute_header

# Disassemble code
otool -tv /tmp/test42
# SHOULD SHOW: movz x0, #0x2a / movz x16, #0x1 / svc #0x80
```

**NOTE:** The `-f` flag on `codesign` is required — without it, codesign may
refuse to replace an existing signature.

### If Phase 0 Fails

| Symptom | Check |
|---------|-------|
| `Killed: 9` | Code signing failed or missing. Run `codesign -f -s -` again. |
| `Bad CPU type in executable` | Mach-O cputype wrong. Must be 0x0100000C. Check endianness. |
| `Segmentation fault: 11` | Entry point wrong. Check entryoff in LC_MAIN matches CODE_OFFSET. |
| `otool: not Mach-O` | Header magic wrong. First 4 bytes must be CF FA ED FE (little-endian). |
| Exit code is 0 instead of 42 | Code not reached. Check CODE_OFFSET calculation. Dump with `xxd /tmp/test42 \| tail`. |
| `Bus error: 10` | Alignment issue. Check __TEXT vmaddr is page-aligned (multiple of 0x4000). |
| codesign "invalid format" | Do NOT pre-allocate LC_CODE_SIGNATURE. Leave 16-byte padding instead. |
| dyld crash on launch | Missing LC_LOAD_DYLIB or LC_DYLD_CHAINED_FIXUPS. All 11 load commands are required. |

### Step 0.3: Save Reference Bytes

```bash
# Save the known-good binary for diffing later
cp /tmp/test42 tools/ref-exit42.macho
xxd tools/ref-exit42.macho > tools/ref-exit42.hex

# These reference files are GOLD. When Phase 2 (macho.fs) fails,
# diff against these to find the problem.
```

**Phase 0 Exit Criteria:** All three tests pass. You have reference binaries saved.

---

## Phase 1: asm.fs — ARM64 Instruction Encoding

**Goal:** A complete ARM64 assembler layer, tested at the byte level,
running on the C engine. No compiler changes. No Mach-O generation yet.

### ARM64 Instruction Encoding Quick Reference

All instructions are 32 bits, little-endian. Fields are packed into a 32-bit word.
Most common pattern: `base_opcode | Rd | (Rn << 5) | (Rm << 16)`.

```
INSTRUCTION          BASE OPCODE    ENCODING
───────────────────  ─────────────  ──────────────────────────────────
ADD  Xd, Xn, Xm     0x8B000000     | Rd | (Rn << 5) | (Rm << 16)
SUB  Xd, Xn, Xm     0xCB000000     | Rd | (Rn << 5) | (Rm << 16)
AND  Xd, Xn, Xm     0x8A000000     | Rd | (Rn << 5) | (Rm << 16)
ORR  Xd, Xn, Xm     0xAA000000     | Rd | (Rn << 5) | (Rm << 16)
EOR  Xd, Xn, Xm     0xCA000000     | Rd | (Rn << 5) | (Rm << 16)
MOV  Xd, Xm         0xAA0003E0     | Rd | (Rm << 16)  (= ORR Xd,XZR,Xm)
NEG  Xd, Xm         0xCB0003E0     | Rd | (Xm << 16)  (= SUB Xd,XZR,Xm)
MVN  Xd, Xm         0xAA2003E0     | Rd | (Xm << 16)  (= ORN Xd,XZR,Xm)
MUL  Xd, Xn, Xm     0x9B007C00     | Rd | (Rn << 5) | (Rm << 16)
SDIV Xd, Xn, Xm     0x9AC00C00     | Rd | (Rn << 5) | (Rm << 16)
UDIV Xd, Xn, Xm     0x9AC00800     | Rd | (Rn << 5) | (Rm << 16)
MSUB Xd,Xn,Xm,Xa    0x9B008000     | Rd | (Rn<<5) | (Rm<<16) | (Xa<<10)

ADD  Xd, Xn, #imm12 0x91000000     | Rd | (Rn << 5) | (imm12 << 10)
SUB  Xd, Xn, #imm12 0xD1000000     | Rd | (Rn << 5) | (imm12 << 10)

CMP  Xn, Xm         0xEB00001F     | (Xn << 5) | (Xm << 16)  (=SUBS XZR)
TST  Xn, Xm         0xEA00001F     | (Xn << 5) | (Xm << 16)  (=ANDS XZR)
CMP  Xn, #imm12     0xF100001F     | (Xn << 5) | (imm12 << 10)

MOVZ Xd, #imm16     0xD2800000     | Rd | (imm16 << 5) | (hw << 21)
MOVK Xd, #imm16     0xF2800000     | Rd | (imm16 << 5) | (hw << 21)
  hw: 0=bits 0-15, 1=bits 16-31, 2=bits 32-47, 3=bits 48-63

LDR  Xt, [Xn, #0]   0xF9400000     | Rt | (Rn << 5)  (unsigned offset)
STR  Xt, [Xn, #0]   0xF9000000     | Rt | (Rn << 5)  (unsigned offset)
LDRB Wt, [Xn]       0x39400000     | Rt | (Rn << 5)
STRB Wt, [Xn]       0x39000000     | Rt | (Rn << 5)

STR  Xt, [Xn, #off]! 0xF8000C00    | Rt | (Rn<<5) | (imm9<<12)  (pre-index)
LDR  Xt, [Xn], #off  0xF8400400    | Rt | (Rn<<5) | (imm9<<12)  (post-index)
STP  Xt1,Xt2,[Xn,#off]! 0xA9800000 | Rt | (Rn<<5) | (Rt2<<10) | (imm7<<15)
LDP  Xt1,Xt2,[Xn],#off  0xA8C00000 | Rt | (Rn<<5) | (Rt2<<10) | (imm7<<15)

B    offset          0x14000000     | (imm26)  offset in 4-byte units
BL   offset          0x94000000     | (imm26)  offset in 4-byte units
RET                  0xD65F03C0     (= BR X30)
BLR  Xn              0xD63F0000     | (Xn << 5)
CBZ  Xn, offset      0xB4000000     | Rn | (imm19 << 5)
CBNZ Xn, offset      0xB5000000     | Rn | (imm19 << 5)
B.cond offset        0x54000000     | cond | (imm19 << 5)
SVC  #0x80           0xD4001001

CSETM Xd, cond       = CSINV Xd, XZR, XZR, invert(cond)
  Base: 0xDA9F03E0   | Rd | (invert(cond) << 12)

LSL  Xd, Xn, #amt   = UBFM Xd, Xn, #(64-amt)&63, #(63-amt)
  Base: 0xD3400000   | Rd | (Rn<<5) | (immr<<16) | (imms<<10)
ASR  Xd, Xn, #amt   = SBFM Xd, Xn, #amt, #63
  Base: 0x93400000   | Rd | (Rn<<5) | (amt<<16) | (63<<10)
LSR  Xd, Xn, #amt   = UBFM Xd, Xn, #amt, #63
  Base: 0xD3400000   | Rd | (Rn<<5) | (amt<<16) | (63<<10)

Condition codes for B.cond and CSETM:
  EQ=0  NE=1  HS=2  LO=3  MI=4  PL=5  VS=6  VC=7
  HI=8  LS=9  GE=10 LT=11 GT=12 LE=13 AL=14
  Invert: flip bit 0 (EQ↔NE, LT↔GE, GT↔LE, etc.)
```

### Step 1.1: Register Constants + emit32

Already done (exists as `arch/arm64/asm.fs`). Verify:

```bash
./engine/fifth compiler/shannon/arch/arm64/asm.fs -e "X19 . cr"
# EXPECTED: 19
./engine/fifth compiler/shannon/arch/arm64/asm.fs -e "LR . cr"
# EXPECTED: 30
```

### Step 1.2: Register-to-Register Operations

Add to `arch/arm64/asm.fs`:

```forth
\ Two-operand interface (matches x86 asm.fs signature)
\ src dst add-rr  means  ADD Xdst, Xdst, Xsrc

: add-rr ( src dst -- )
  swap 16 lshift over 5 lshift or swap or
  $8B000000 or emit32 ;

: sub-rr ( src dst -- )
  swap 16 lshift over 5 lshift or swap or
  $CB000000 or emit32 ;

: and-rr ( src dst -- )
  swap 16 lshift over 5 lshift or swap or
  $8A000000 or emit32 ;

: or-rr ( src dst -- )
  swap 16 lshift over 5 lshift or swap or
  $AA000000 or emit32 ;

: xor-rr ( src dst -- )
  swap 16 lshift over 5 lshift or swap or
  $CA000000 or emit32 ;

: cmp-rr ( src dst -- )
  \ CMP Xdst, Xsrc = SUBS XZR, Xdst, Xsrc
  swap 16 lshift swap 5 lshift or 31 or
  $EB000000 or emit32 ;

: test-rr ( src dst -- )
  \ TST Xdst, Xsrc = ANDS XZR, Xdst, Xsrc
  swap 16 lshift swap 5 lshift or 31 or
  $EA000000 or emit32 ;

: mov-rr ( src dst -- )
  \ MOV Xdst, Xsrc = ORR Xdst, XZR, Xsrc
  swap 16 lshift or $AA0003E0 or emit32 ;

: imul-rr ( src dst -- )
  \ MUL Xdst, Xdst, Xsrc
  swap 16 lshift over 5 lshift or swap or
  $9B007C00 or emit32 ;

: xchg-rr ( r1 r2 -- )
  \ Swap r1 ↔ r2 via X9 scratch
  over X9 mov-rr          \ MOV X9, r1
  dup rot mov-rr           \ MOV r1, r2
  X9 swap mov-rr ;         \ MOV r2, X9
```

**Test:**
```bash
# Write a test that emits instructions and checks the bytes
cat > /tmp/test-asm-arm64.fs << 'EOF'
include compiler/shannon/arch/arm64/asm.fs

\ Allocate a buffer to emit into
create test-buf 256 allot
variable test-pos  0 test-pos !

\ Override c, to write to test-buf
: c, ( b -- ) test-buf test-pos @ + c!  1 test-pos +! ;

\ Test: ADD X19, X19, X20 should be 0x8B140273
0 test-pos !
X20 X19 add-rr
\ Read back the 4 bytes
test-buf c@ test-buf 1+ c@ 8 lshift or
test-buf 2 + c@ 16 lshift or test-buf 3 + c@ 24 lshift or
$8B140273 = if ." add-rr: PASS" else ." add-rr: FAIL" then cr

\ Test: MOV X19, X20 should be 0xAA1403F3
0 test-pos !
X20 X19 mov-rr
test-buf c@ test-buf 1+ c@ 8 lshift or
test-buf 2 + c@ 16 lshift or test-buf 3 + c@ 24 lshift or
$AA1403F3 = if ." mov-rr: PASS" else ." mov-rr: FAIL" then cr

bye
EOF
./engine/fifth /tmp/test-asm-arm64.fs
# EXPECTED:
# add-rr: PASS
# mov-rr: PASS
```

### Step 1.3: Immediate Operations

```forth
: add-ri ( imm reg -- )
  \ ADD Xreg, Xreg, #imm12 (imm must fit in 12 bits)
  swap $FFF and 10 lshift         \ imm12 << 10
  over 5 lshift or swap or
  $91000000 or emit32 ;

: sub-ri ( imm reg -- )
  swap $FFF and 10 lshift
  over 5 lshift or swap or
  $D1000000 or emit32 ;

: add-ri8 ( imm reg -- ) add-ri ;   \ ARM64 doesn't need 8-bit special case
: sub-ri8 ( imm reg -- ) sub-ri ;

: cmp-ri ( imm reg -- )
  \ CMP Xreg, #imm12
  swap $FFF and 10 lshift
  swap 5 lshift or 31 or
  $F1000000 or emit32 ;
```

### Step 1.4: Unary Operations

```forth
: neg-r ( reg -- )
  \ NEG Xd, Xd = SUB Xd, XZR, Xd
  dup 16 lshift swap or $CB0003E0 or emit32 ;

: not-r ( reg -- )
  \ MVN Xd, Xd = ORN Xd, XZR, Xd
  dup 16 lshift swap or $AA2003E0 or emit32 ;

: inc-r ( reg -- )
  \ ADD Xd, Xd, #1
  dup 5 lshift or $91000400 or emit32 ;

: dec-r ( reg -- )
  \ SUB Xd, Xd, #1
  dup 5 lshift or $D1000400 or emit32 ;
```

### Step 1.5: Immediates (MOVZ/MOVK)

```forth
: movz ( imm16 hw reg -- )
  \ MOVZ Xd, #imm16, LSL #(hw*16)
  >r swap 21 lshift swap $FFFF and 5 lshift or r> or
  $D2800000 or emit32 ;

: movk ( imm16 hw reg -- )
  \ MOVK Xd, #imm16, LSL #(hw*16)
  >r swap 21 lshift swap $FFFF and 5 lshift or r> or
  $F2800000 or emit32 ;

: mov-ri ( imm reg -- )
  \ Load arbitrary 64-bit immediate into Xreg using MOVZ + up to 3 MOVKs
  swap dup $FFFF and 0 3 pick movz
  dup 16 rshift $FFFF and ?dup if 1 3 pick movk then
  dup 32 rshift $FFFF and ?dup if 2 3 pick movk then
  dup 48 rshift $FFFF and ?dup if 3 3 pick movk then
  drop drop ;
```

**Test:** Emit MOVZ X19, #42 and verify bytes = 0xD2800553:
```
42 (= 0x2A) << 5 = 0x540, | 19 = 0x553
0xD2800000 | 0x553 = 0xD2800553
Little-endian bytes: 53 05 80 D2
```

### Step 1.6: Memory Operations

```forth
: ldr-r[r] ( rt rn -- )
  \ LDR Xt, [Xn] (zero offset, unsigned)
  5 lshift or $F9400000 or emit32 ;

: str-r[r] ( rt rn -- )
  \ STR Xt, [Xn]
  5 lshift or $F9000000 or emit32 ;

: ldrb-r[r] ( rt rn -- )
  \ LDRB Wt, [Xn]
  5 lshift or $39400000 or emit32 ;

: strb-r[r] ( rt rn -- )
  \ STRB Wt, [Xn]
  5 lshift or $39000000 or emit32 ;

: str-pre ( rt rn imm9 -- )
  \ STR Xt, [Xn, #imm9]!  (pre-index, for stack push)
  $1FF and 12 lshift rot 5 lshift or rot or
  $F8000C00 or emit32 ;

: ldr-post ( rt rn imm9 -- )
  \ LDR Xt, [Xn], #imm9  (post-index, for stack pop)
  $1FF and 12 lshift rot 5 lshift or rot or
  $F8400400 or emit32 ;

: ldr-offset ( rt rn imm12 -- )
  \ LDR Xt, [Xn, #imm12]  (unsigned offset, scaled by 8)
  3 rshift $FFF and 10 lshift rot 5 lshift or rot or
  $F9400000 or emit32 ;

: str-offset ( rt rn imm12 -- )
  \ STR Xt, [Xn, #imm12]  (unsigned offset, scaled by 8)
  3 rshift $FFF and 10 lshift rot 5 lshift or rot or
  $F9000000 or emit32 ;

: stp-pre ( rt1 rt2 rn imm7 -- )
  \ STP Xt1, Xt2, [Xn, #imm7*8]!  (pre-index, for saving reg pairs)
  3 rshift $7F and 15 lshift rot 10 lshift or rot 5 lshift or rot or
  $A9800000 or emit32 ;

: ldp-post ( rt1 rt2 rn imm7 -- )
  \ LDP Xt1, Xt2, [Xn], #imm7*8  (post-index, for restoring reg pairs)
  3 rshift $7F and 15 lshift rot 10 lshift or rot 5 lshift or rot or
  $A8C00000 or emit32 ;
```

### Step 1.7: Branches

```forth
: emit-b ( -- addr )
  \ Emit B (unconditional branch) with placeholder offset, return patch addr
  $14000000 emit32
  code-here ;            \ address AFTER the instruction

: emit-bl ( -- addr )
  \ Emit BL with placeholder
  $94000000 emit32
  code-here ;

: emit-ret ( -- )
  $D65F03C0 emit32 ;

: emit-blr ( reg -- )
  \ BLR Xreg (call via register)
  5 lshift $D63F0000 or emit32 ;

: emit-cbz ( reg -- addr )
  \ CBZ Xreg, placeholder
  $B4000000 or emit32
  code-here ;

: emit-cbnz ( reg -- addr )
  \ CBNZ Xreg, placeholder
  $B5000000 or emit32
  code-here ;

: emit-bcond ( cond -- addr )
  \ B.cond placeholder
  $54000000 or emit32
  code-here ;

: emit-svc ( -- )
  \ SVC #0x80
  $D4001001 emit32 ;
```

### Step 1.8: Branch Patching

This is critical. ARM64 branch offsets are encoded INTO the instruction, not after it.

```forth
: d@ ( addr -- n )
  \ Read 32-bit little-endian from code-buf
  dup c@ swap 1+
  dup c@ 8 lshift swap 1+
  dup c@ 16 lshift swap 1+
  c@ 24 lshift or or or ;

: patch-b ( target-cpos instr-cpos -- )
  \ Patch B or BL instruction at instr-cpos (code-buf offset)
  \ target-cpos is also a code-buf offset
  \ The instruction is at instr-cpos - 4 (we stored addr AFTER the insn)
  swap over 4 - - 4 /         \ offset in instructions
  $3FFFFFF and                 \ 26-bit mask
  swap 4 - dup >r              \ instr address in code-buf
  code-buf + d@                \ read current instruction
  $FC000000 and                \ keep opcode bits
  rot or                       \ insert new offset
  r> code-buf + d! ;           \ write back

: patch-bcond ( target-cpos instr-cpos -- )
  \ Patch B.cond, CBZ, or CBNZ instruction
  \ imm19 is in bits 23:5
  swap over 4 - - 4 /         \ offset in instructions
  $7FFFF and 5 lshift          \ 19-bit mask, positioned at bits 23:5
  swap 4 - dup >r
  code-buf + d@
  $FF00001F and                \ keep opcode + Rt/cond bits (clear imm19)
  rot or
  r> code-buf + d! ;
```

**Test each instruction** against known encodings from the ARM64 manual. Write a
comprehensive `test-asm-arm64.fs` that verifies at least these:

```
ADD X19, X19, X20  → 0x8B140273
SUB X19, X20, X19  → 0xCB130293
MOV X19, X20       → 0xAA1403F3
NEG X19, X19       → 0xCB1303F3
MOVZ X19, #42      → 0xD2800553
RET                → 0xD65F03C0
SVC #0x80          → 0xD4001001
CMP X19, X20       → 0xEB140273  (wait: SUBS XZR, X19, X20)
```

Let me verify CMP:
CMP X19, X20 = SUBS XZR, X19, X20
= 0xEB000000 | 31 | (19 << 5) | (20 << 16)
= 0xEB000000 | 0x1F | 0x260 | 0x140000
= 0xEB14027F

So the test should be: `X20 X19 cmp-rr` → 0xEB14027F.

**Phase 1 Exit Criteria:** Every instruction passes its byte-level test.
The test file runs on the C engine without errors.

---

## Phase 2: macho.fs — Binary Format

**Goal:** Generate runnable Mach-O executables from Forth code.

### Step 2.1: Minimal macho.fs

Create `arch/arm64/macho.fs` that generates the same layout as `tools/macho-test.c`.
This means LC_MAIN + dynamic linking with all 11 load commands.

```forth
\ macho.fs - Mach-O ARM64 binary generation
\ Replaces elf.fs for ARM64 target
\ Layout matches tools/macho-test.c exactly

create macho-buf 1024 allot
variable macho-pos  0 macho-pos !

: m,  ( b -- )       macho-buf macho-pos @ + c!  1 macho-pos +! ;
: m2, ( word -- )     dup m, 8 rshift m, ;
: m4, ( dword -- )    dup m2, 16 rshift m2, ;
: m8, ( qword -- )    dup m4, 32 rshift m4, ;
: m-str ( c-addr u field-size -- )
  \ Write string padded to field-size
  >r 2dup r@ min 0 do dup i + c@ m, loop
  dup r> swap - 0 ?do 0 m, loop 2drop ;
: m-zero ( n -- )
  \ Write n zero bytes
  0 ?do 0 m, loop ;

$100000000 constant CODE-BASE  \ standard Mach-O text base
$4000 constant PAGE-SIZE       \ 16KB pages on Apple Silicon

\ Layout constants (must match tools/macho-test.c)
32 constant /header
576 constant /sizeofcmds       \ total size of all 11 load commands
11 constant /ncmds
16 constant /codesign-pad      \ space for codesign to insert LC_CODE_SIGNATURE
/header /sizeofcmds + /codesign-pad + constant /code-offset  \ = 624

\ LINKEDIT layout
56 constant /chained-fixups
48 constant /exports-trie
32 constant /nlist             \ 2 symbols × 16 bytes
40 constant /strtab
/chained-fixups /exports-trie + /nlist + /strtab + constant /linkedit-data  \ = 176
336 constant /linkedit-pad     \ codesign appends signature here
/linkedit-data /linkedit-pad + constant /linkedit-fsize  \ = 512

: macho-header ( code-size -- )
  0 macho-pos !
  >r

  \ --- Mach-O Header (32 bytes) ---
  $FEEDFACF m4,            \ magic
  $0100000C m4,            \ cputype = CPU_TYPE_ARM64
  0 m4,                    \ cpusubtype
  2 m4,                    \ filetype = MH_EXECUTE
  /ncmds m4,               \ ncmds = 11
  /sizeofcmds m4,          \ sizeofcmds = 576
  $00200085 m4,            \ flags: MH_NOUNDEFS|MH_DYLDLINK|MH_TWOLEVEL|MH_PIE
  0 m4,                    \ reserved

  \ --- LC_SEGMENT_64 __PAGEZERO (72 bytes) ---
  $19 m4,  72 m4,
  s" __PAGEZERO" 16 m-str
  0 m8,  $100000000 m8,    \ vmaddr=0, vmsize=4GB
  0 m8,  0 m8,             \ fileoff, filesize
  0 m4,  0 m4,             \ maxprot, initprot
  0 m4,  0 m4,             \ nsects, flags

  \ --- LC_SEGMENT_64 __TEXT + __text section (152 bytes) ---
  $19 m4,  152 m4,
  s" __TEXT" 16 m-str
  CODE-BASE m8,             \ vmaddr
  PAGE-SIZE m8,             \ vmsize = one page
  0 m8,                    \ fileoff = 0
  PAGE-SIZE m8,             \ filesize = one page
  5 m4,  5 m4,             \ maxprot=r-x, initprot=r-x
  1 m4,  0 m4,             \ nsects=1, flags=0
  \ section: __text
  s" __text" 16 m-str
  s" __TEXT" 16 m-str
  CODE-BASE /code-offset + m8,  \ addr (virtual)
  r@ m8,                   \ size
  /code-offset m4,         \ offset = 624
  2 m4,                    \ align = 2^2 = 4 bytes
  0 m4,  0 m4,             \ reloff, nreloc
  $80000400 m4,            \ flags = PURE_INSTRUCTIONS | SOME_INSTRUCTIONS
  0 m4,  0 m4,  0 m4,     \ reserved

  \ --- LC_SEGMENT_64 __LINKEDIT (72 bytes) ---
  $19 m4,  72 m4,
  s" __LINKEDIT" 16 m-str
  CODE-BASE PAGE-SIZE + m8,  \ vmaddr = CODE-BASE + PAGE-SIZE
  PAGE-SIZE m8,             \ vmsize = one page
  PAGE-SIZE m8,             \ fileoff = PAGE-SIZE
  /linkedit-fsize m8,       \ filesize = 512
  1 m4,  1 m4,             \ maxprot=r, initprot=r
  0 m4,  0 m4,             \ nsects, flags

  \ --- LC_LOAD_DYLINKER (32 bytes) ---
  $E m4,  32 m4,           \ cmd, cmdsize
  12 m4,                   \ offset to path string
  s" /usr/lib/dyld" 20 m-str

  \ --- LC_BUILD_VERSION (32 bytes) ---
  $32 m4,  32 m4,          \ cmd, cmdsize
  1 m4,                    \ platform = macOS
  $000F0000 m4,            \ minos = 15.0
  $000F0100 m4,            \ sdk = 15.1
  0 m4,                    \ ntools
  0 m4,  0 m4,             \ padding to 32

  \ --- LC_MAIN (24 bytes) ---
  $80000028 m4,  24 m4,    \ cmd, cmdsize
  /code-offset m8,          \ entryoff = 624
  0 m8,                    \ stacksize = 0 (default)

  \ --- LC_LOAD_DYLIB libSystem (56 bytes) ---
  $C m4,  56 m4,           \ cmd, cmdsize
  24 m4,                   \ offset to name
  0 m4,                    \ timestamp
  $050001FF m4,            \ current_version
  1 m4,                    \ compat_version
  s" /usr/lib/libSystem.B.dylib" 32 m-str

  \ --- LC_SYMTAB (24 bytes) ---
  $2 m4,  24 m4,           \ cmd, cmdsize
  PAGE-SIZE /chained-fixups + /exports-trie + m4,  \ symoff
  2 m4,                    \ nsyms
  PAGE-SIZE /chained-fixups + /exports-trie + /nlist + m4,  \ stroff
  /strtab m4,              \ strsize

  \ --- LC_DYSYMTAB (80 bytes) ---
  $B m4,  80 m4,           \ cmd, cmdsize
  \ 18 fields, all zero for minimal binary
  18 0 do 0 m4, loop

  \ --- LC_DYLD_CHAINED_FIXUPS (16 bytes) ---
  $80000034 m4,  16 m4,    \ cmd, cmdsize
  PAGE-SIZE m4,             \ dataoff (start of LINKEDIT)
  /chained-fixups m4,       \ datasize = 56

  \ --- LC_DYLD_EXPORTS_TRIE (16 bytes) ---
  $80000033 m4,  16 m4,    \ cmd, cmdsize
  PAGE-SIZE /chained-fixups + m4,  \ dataoff
  /exports-trie m4,         \ datasize = 48

  \ --- 16 bytes padding for codesign ---
  /codesign-pad m-zero

  r> drop ;

\ Emit the LINKEDIT data (chained fixups, exports trie, symtab, strtab)
: emit-linkedit ( fid -- )
  >r

  \ --- Chained fixups (56 bytes) ---
  \ dyld_chained_fixups_header
  0 m4,  32 m4,  48 m4,  48 m4,   \ fixups_version, starts_offset, imports_offset, symbols_offset
  0 m4,  1 m4,  0 m4,  0 m4,      \ imports_count, imports_format, symbols_format, pad
  \ dyld_chained_starts_in_image
  3 m4,  0 m4,  0 m4,  0 m4,      \ seg_count, seg_info_offset[3] all zero
  0 m4,  0 m4,                     \ padding to 56

  \ --- Exports trie (48 bytes) ---
  \ Root: terminal_size=0, 1 child, edge "_", child offset 5
  0 m,  1 m,  [char] _ m,  0 m,  5 m,
  \ N1: terminal_size=0, 2 children
  0 m,  2 m,
  s" _mh_execute_header" dup 0 do over i + c@ m, loop 2drop  0 m,  $21 m,
  s" main" dup 0 do over i + c@ m, loop 2drop  0 m,  $25 m,
  \ N2 (__mh_execute_header @ addr 0): terminal_size=2, flags=0, addr=0 ULEB128
  2 m,  0 m,  0 m,  0 m,
  \ N3 (_main @ addr 624): terminal_size=3, flags=0, addr=0xF0 0x04 ULEB128
  3 m,  0 m,  $F0 m,  4 m,  0 m,
  \ Pad to 48 bytes
  \ (calculate remaining and pad)

  \ --- nlist_64 entries (32 bytes) ---
  \ Symbol 0: __mh_execute_header
  1 m4,  $0F m,  1 m,  0 m2,  0 m8,
  \ Symbol 1: _main
  $15 m4,  $0F m,  1 m,  0 m2,
  CODE-BASE /code-offset + m8,

  \ --- String table (40 bytes) ---
  \ Byte 0: null
  0 m,
  s" __mh_execute_header" dup 0 do over i + c@ m, loop 2drop  0 m,
  s" _main" dup 0 do over i + c@ m, loop 2drop  0 m,
  \ Pad to 40 bytes

  \ --- Padding (336 bytes) ---
  /linkedit-pad m-zero

  \ Write LINKEDIT from macho-buf
  macho-buf macho-pos @ r> write-file throw ;

: write-macho ( filename-addr filename-len code-size -- )
  dup macho-header
  >r 2dup
  w/o create-file throw >r

  \ Write header + load commands + padding (macho-buf)
  macho-buf macho-pos @ r@ write-file throw

  \ Write code from code-buf
  code-buf code-pos @ r@ write-file throw

  \ Pad to PAGE-SIZE
  \ ... (write zeros to fill to page boundary)

  \ Write LINKEDIT
  0 macho-pos !  \ reuse macho-buf for LINKEDIT
  r@ emit-linkedit

  r> close-file throw
  r> drop ;
```

**Important:** The Forth version MUST produce byte-identical headers to
`tools/macho-test.c`. Diff the hex dumps to verify. Any byte difference
in the header means macho.fs has a bug.

### Step 2.2: Test — Exit-Only Binary from Forth

```bash
cat > /tmp/test-macho.fs << 'EOF'
\ Test: Generate a Mach-O binary that exits with code 42

include compiler/shannon/arch/arm64/asm.fs

\ Code emission infrastructure (same as main.fs)
262144 constant CODE-SIZE
create code-buf CODE-SIZE allot
variable code-pos  0 code-pos !
: c, ( b -- ) code-buf code-pos @ + c!  1 code-pos +! ;
: code-here ( -- n ) code-pos @ ;

include compiler/shannon/arch/arm64/macho.fs

\ Emit ARM64 code: exit(42)
42 0 X0 movz             \ MOVZ X0, #42
1 0 X16 movz             \ MOVZ X16, #1
emit-svc                  \ SVC #0x80

\ Write the Mach-O
s" /tmp/test-macho-exit42" code-pos @ write-macho

bye
EOF

./engine/fifth /tmp/test-macho.fs
chmod +x /tmp/test-macho-exit42
codesign -f -s - /tmp/test-macho-exit42
/tmp/test-macho-exit42; echo $?
# EXPECTED: 42
```

### Step 2.3: Diff Against Reference

```bash
xxd /tmp/test-macho-exit42 > /tmp/test-macho-exit42.hex
diff tools/ref-exit42.hex /tmp/test-macho-exit42.hex
# If there are differences in the header, the problem is in macho.fs.
# If there are differences in the code section, the problem is in asm.fs.
```

### If Phase 2 Fails

| Symptom | Check |
|---------|-------|
| `Killed: 9` | codesign failed. Check file size is correct (not truncated). |
| Wrong exit code | Code bytes are wrong. `otool -tv` to disassemble. |
| `Malformed Mach-O` | Load command sizes don't add up. `otool -l` to inspect. |
| Hangs | No exit syscall. Check code was actually written to file. |

**Phase 2 Exit Criteria:** Forth-generated Mach-O binary exits with code 42.
Matches Phase 0 reference binary byte-for-byte in the header.

---

## Phase 3: stack.fs — Register-Mapped Stack

**Goal:** Port the stack machine. Test with standalone binary generation.

### The Stack Model

Identical to x86 except for register names and instruction widths:

```
Items on stack:    Where they live:
  1                X19 (TOS)
  2                X19, X20 (TOS, NOS)
  3                X19, X20, X21 (TOS, NOS, 3rd)
  4+               X19, X20, X21, then memory at [X22] growing down
```

### Key Words

```forth
\ stack.fs for ARM64

variable stack-depth  0 stack-depth !
variable dead-code    0 dead-code !

: tos ( -- reg ) X19 ;
: nos ( -- reg ) X20 ;
: third ( -- reg ) X21 ;
: stkptr ( -- reg ) X22 ;

\ Spill/fill for memory stack
: spill-third ( -- )
  \ STR X21, [X22, #-8]!  (pre-index: decrement then store)
  X21 X22 -8 str-pre ;

: fill-third ( -- )
  \ LDR X21, [X22], #8  (post-index: load then increment)
  X21 X22 8 ldr-post ;

: push-val ( -- )
  stack-depth @
  dup 3 >= if spill-third then
  dup 2 >= if X20 X21 mov-rr then    \ MOV X21, X20
  drop
  X19 X20 mov-rr                      \ MOV X20, X19
  1 stack-depth+! ;

: pop-val ( -- )
  X20 X19 mov-rr                      \ MOV X19, X20
  stack-depth @ 1-
  dup 2 >= if X21 X20 mov-rr then    \ MOV X20, X21
  dup 3 >= if fill-third then
  drop
  -1 stack-depth+! ;

: pop-nos-val ( -- )
  stack-depth @ 1-
  dup 2 >= if X21 X20 mov-rr then
  dup 3 >= if fill-third then
  drop
  -1 stack-depth+! ;
```

### emit-lit for ARM64

```forth
: emit-lit ( n -- )
  push-val
  dup 0= if
    drop
    \ MOV X19, XZR = zero
    XZR X19 mov-rr
  else dup $FFFF <= over 0>= and if
    \ Small positive: MOVZ X19, #imm16
    0 tos movz
  else
    \ Full 64-bit: MOVZ + up to 3 MOVK
    tos mov-ri
  then then ;
```

### Test

```bash
cat > /tmp/test-stack.fs << 'EOF'
\ Test stack: push 42, exit with it

include compiler/shannon/arch/arm64/asm.fs
\ ... (code emission infrastructure) ...
include compiler/shannon/arch/arm64/stack.fs
include compiler/shannon/arch/arm64/macho.fs

\ Push 42 onto Forth stack (goes into X19)
42 emit-lit

\ Exit with TOS (X19) as exit code
\ MOV X0, X19
X19 X0 mov-rr
1 0 X16 movz
emit-svc

s" /tmp/test-stack" code-pos @ write-macho
bye
EOF

./engine/fifth /tmp/test-stack.fs
chmod +x /tmp/test-stack && codesign -f -s - /tmp/test-stack
/tmp/test-stack; echo $?
# EXPECTED: 42
```

### Test Stack Cascade

```bash
# Push 5 values, pop them, compute sum, exit with it
# Push 1, 2, 3, 4, 5 (5 goes to X19, 4 to X20, 3 to X21, 2 to [X22], 1 to [X22-8])
# Pop them back and add: should get 15

# This test verifies the memory overflow stack works
```

**Phase 3 Exit Criteria:** Push/pop/emit-lit work correctly. Stack cascade
to memory and back produces correct values.

---

## Phase 4: prims.fs — Primitive Codegen

**Goal:** Port every emit-* word. Test each one with exit codes.

### Translation Table

Each x86 emit-* word maps to ARM64 instructions. The stack interface (push-val, pop-val,
pop-nos-val) stays the same — it just uses different registers now.

```
WORD            x86 INSTRUCTIONS              ARM64 INSTRUCTIONS
──────────────  ────────────────────────────  ──────────────────────────────
emit-add        add rax, rbx; pop-nos-val    ADD X19,X19,X20; pop-nos-val
emit-sub        sub rbx,rax; mov rax,rbx;    SUB X19,X20,X19; pop-nos-val
                pop-nos-val
emit-mul        imul rax,rbx; pop-nos-val    MUL X19,X19,X20; pop-nos-val
emit-negate     neg rax                       NEG X19, X19
emit-1+         inc rax                       ADD X19, X19, #1
emit-1-         dec rax                       SUB X19, X19, #1
emit-and        and rax,rbx; pop-nos-val     AND X19,X19,X20; pop-nos-val
emit-or         or rax,rbx; pop-nos-val      ORR X19,X19,X20; pop-nos-val
emit-xor        xor rax,rbx; pop-nos-val    EOR X19,X19,X20; pop-nos-val
emit-invert     not rax                       MVN X19, X19
emit-dup        push-val                      push-val (same!)
emit-drop       pop-val                       pop-val (same!)
emit-swap       xchg rax,rbx                 xchg-rr (3 MOVs via X9)
emit-over       push-val; mov rax,rbx        push-val; MOV X19,X20
emit-@          mov rax,[rax]                 LDR X19, [X19]
emit-!          mov [rax],rbx;pop;pop        STR X20, [X19]; pop-val; pop-val
emit-c@         movzx rax,byte[rax]          LDRB W19, [X19]; (zero-extends to X19)
emit-c!         mov byte[rax],bl;pop;pop     STRB W20, [X19]; pop-val; pop-val
emit-=          cmp+setz+movzx+neg           CMP X20,X19; CSETM X19,EQ; pop-nos
emit-<          cmp+setl+movzx+neg           CMP X20,X19; CSETM X19,LT; pop-nos
emit->          cmp+setg+movzx+neg           CMP X20,X19; CSETM X19,GT; pop-nos
emit-0=         test+setz+movzx+neg          CMP X19,#0; CSETM X19,EQ
emit-0<         sar rax,63                    ASR X19, X19, #63
emit-/          cqo;idiv rbx;pop-nos         SDIV X19,X20,X19; pop-nos-val
emit-mod        cqo;idiv;mov rax,rdx;pop-nos SDIV X9,X20,X19; MSUB X19,X9,X19,X20; pop-nos
emit-bye        mov eax,60;xor edi;syscall   MOVZ X0,#0; MOVZ X16,#1; SVC
```

### ARM64 Comparison Pattern

x86 uses SETcc + MOVZX + NEG (3 instructions, ~10 bytes) to produce -1/0 flags.
ARM64 uses CSETM (1 instruction, 4 bytes) which directly produces -1 or 0.

```forth
: emit-= ( -- )
  nos tos cmp-rr              \ CMP X19, X20 (sets flags)
  pop-nos-val
  \ CSETM X19, EQ = CSINV X19, XZR, XZR, NE
  $DA9F13F3 emit32 ;          \ NE = 1 (inverted EQ)

: emit-< ( -- )
  nos tos cmp-rr              \ CMP X20, X19 (NOS vs TOS)
  pop-nos-val
  \ CSETM X19, LT = CSINV X19, XZR, XZR, GE
  $DA9FA3F3 emit32 ;          \ GE = 10 = 0xA

: emit-> ( -- )
  nos tos cmp-rr
  pop-nos-val
  \ CSETM X19, GT = CSINV X19, XZR, XZR, LE
  $DA9FD3F3 emit32 ;          \ LE = 13 = 0xD
```

Wait — let me double-check the CMP operand order. On x86, `cmp rbx, rax` compares
NOS against TOS (rbx - rax). For `emit-<` we want "NOS < TOS" which is true when
rbx - rax is negative (signed less). The x86 SETL captures this.

On ARM64, `CMP X20, X19` computes X20 - X19 and sets flags. "NOS < TOS" means
X20 < X19, which means X20 - X19 is negative, which means the LT condition is true.
So `CSETM X19, LT` is correct.

But wait: the `cmp-rr ( src dst -- )` interface has src=X20, dst=X19, meaning it
emits `CMP X19, X20` (SUBS XZR, X19, X20). That computes X19 - X20, NOT X20 - X19.

This means the comparison is backwards! We need `CMP X20, X19` for NOS vs TOS.
The fix: swap the operands to cmp-rr, or write a dedicated compare word:

```forth
: cmp-nos-tos ( -- )
  \ CMP NOS, TOS = SUBS XZR, X20, X19
  tos nos cmp-rr ;   \ src=X19, dst=X20 → CMP X20, X19 ✓
```

This matches the x86 version where `cmp-rr` is called as `tos nos cmp-rr` in prims.fs.

### Test Each Primitive

Test pattern: write a tiny .fs file that uses one primitive, exits with result.

```bash
# Test emit-add: 35 + 7 = 42
# Generate code that: push 35, push 7, add, exit with TOS
# ... (using standalone test harness)
# EXPECTED: exit code 42

# Test emit-sub: 49 - 7 = 42
# EXPECTED: exit code 42

# Test emit-mul: 6 * 7 = 42
# EXPECTED: exit code 42

# Test emit-=: 5 5 = → -1 → exit code 255 (low byte of -1)
# EXPECTED: exit code 255

# Test emit-0<: -1 0< → -1 → exit code 255
# EXPECTED: exit code 255
```

**Phase 4 Exit Criteria:** All emit-* words produce correct results via exit codes.

---

## Phase 5: control.fs — Branches

**Goal:** Port if/then/else, begin/while/repeat, do/loop.

### ARM64 Branch Encoding Differences from x86

| Aspect | x86 | ARM64 |
|--------|-----|-------|
| Instruction size | Variable (2-6 bytes) | Fixed (4 bytes) |
| Offset unit | Bytes | Instructions (÷4) |
| Offset location | After opcode bytes | Inside the instruction word |
| Forward patch | Write 4-byte displacement at addr-4 | Read-modify-write instruction at addr-4 |

### gen-if on ARM64

x86 version: `test rdi,rdi; jz rel32` (9 bytes for test + 6 bytes for jz = 15 bytes)
ARM64 version: `CBZ X19, offset` (4 bytes) + pop-val

```forth
: gen-if ( -- orig )
  \ Test TOS, branch if zero, pop TOS
  \ Save TOS to scratch, pop, then CBZ scratch
  X19 X9 mov-rr              \ MOV X9, X19 (save TOS)
  pop-val
  X9 emit-cbz ;              \ CBZ X9, placeholder → returns addr to patch

: gen-then ( orig -- )
  code-here swap patch-bcond ;

: gen-else ( orig1 -- orig2 )
  emit-b                      \ B placeholder → returns new orig
  swap
  code-here swap patch-bcond ;
```

### gen-begin / gen-until / gen-again

```forth
: gen-begin ( -- dest )
  code-here ;

: gen-until ( dest -- )
  X19 X9 mov-rr              \ save TOS
  pop-val
  \ CBZ X9, dest (branch back if zero)
  code-here 4 + over - 4 /   \ offset in instructions
  $7FFFF and 5 lshift         \ imm19 positioned
  X9 or $B4000000 or emit32
  drop ;

: gen-again ( dest -- )
  \ B dest
  code-here 4 + over - 4 /   \ offset (negative for backward)
  $3FFFFFF and $14000000 or emit32
  drop ;
```

### DO/LOOP on ARM64

Uses X28 (rstack pointer) instead of RBP.
Loop index and limit stored at [X28] and [X28+8].

```forth
: gen-do ( -- do-addr leave-addr )
  leave-mark
  \ SUB X28, X28, #16
  16 X28 sub-ri
  \ STR X20, [X28, #8]  (limit = NOS)
  X20 X28 8 str-offset
  \ STR X19, [X28]  (index = TOS)
  X19 X28 0 str-offset
  pop-val pop-val
  code-here  0 ;

: gen-loop ( do-addr leave-addr -- )
  \ LDR X9, [X28]       (index)
  X9 X28 0 ldr-offset
  \ ADD X9, X9, #1      (increment)
  X9 inc-r
  \ STR X9, [X28]       (store back)
  X9 X28 0 str-offset
  \ LDR X10, [X28, #8]  (limit)
  X10 X28 8 ldr-offset
  \ CMP X9, X10
  X10 X9 cmp-rr
  \ B.LT do-addr
  code-here 4 + 4 pick - 4 /    \ offset to do-addr
  $7FFFF and 5 lshift
  11 or                           \ LT condition = 0xB
  $54000000 or emit32
  \ ADD X28, X28, #16  (cleanup)
  16 X28 add-ri
  \ Patch leave-addr if non-zero
  swap ?dup if code-here swap patch-bcond then
  drop
  leave-patch ;

: gen-i ( -- )
  push-val
  \ LDR X19, [X28]
  X19 X28 0 ldr-offset ;

: gen-j ( -- )
  push-val
  \ LDR X19, [X28, #16]
  X19 X28 16 ldr-offset ;
```

### Test

```bash
# Test if/then: 1 if 42 else 0 then → exit 42
# Test begin/until: count from 10 down to 0 → exit 0
# Test do/loop: sum 1 to 10 → exit 55
```

**Phase 5 Exit Criteria:** if/then/else, begin/until/again, do/loop all produce
correct exit codes.

---

## Phase 6: io.fs — System Calls

**Goal:** Port I/O operations using macOS ARM64 syscall convention.

### gen-emit on ARM64

x86 version: ~20 bytes of inline code saving registers around syscall.
ARM64 version: simpler because TOS/NOS/3rd are callee-saved.

```forth
: gen-emit ( -- )
  \ ( c -- ) Write one character to stdout
  \ X19 = character. Need to put it on the real stack for write() buffer.
  \ SUB SP, SP, #16 (16-byte aligned)
  $D10043FF emit32
  \ STRB W19, [SP]  (store character byte)
  X19 31 strb-r[r]     \ SP encoding = 31
  \ MOV X0, #1 (stdout)
  1 0 X0 movz
  \ MOV X1, SP (buffer)
  $910003E1 emit32       \ ADD X1, SP, #0 = MOV X1, SP
  \ MOV X2, #1 (length)
  1 0 X2 movz
  \ MOV X16, #4 (write syscall)
  4 0 X16 movz
  \ SVC #0x80
  emit-svc
  \ ADD SP, SP, #16
  $910043FF emit32
  \ pop-val (consume the character)
  pop-val ;
```

No register saves needed! X19/X20/X21 survive the SVC because they're callee-saved.
This is 9 ARM64 instructions (36 bytes) vs x86's ~25 bytes with push/pop overhead.

### gen-dot (Print Number)

This is the most complex I/O word. On x86 it's ~80 bytes of inline code.
On ARM64 it will be similar — the algorithm is the same, just different registers.

The algorithm:
1. Check sign, print '-' if negative, negate
2. Repeatedly divide by 10, push digits to SP
3. Print digits from SP, pop as you go
4. Print trailing space

```forth
: gen-dot ( -- )
  \ ( n -- ) Print signed number with trailing space
  \ This is complex inline code. Key: X19=TOS (the number to print)

  \ TODO: Implement division loop using SDIV/MSUB
  \ For now, this is the implementation outline:
  \   1. MOV X9, X19 (save number)
  \   2. pop-val (consume from stack)
  \   3. Check sign: CMP X9, #0; B.GE positive
  \   4. Print '-': SUB SP,SP,#16; MOV W10,#45; STRB W10,[SP]; write(1,SP,1); ADD SP,SP,#16
  \   5. NEG X9, X9
  \   6. Division loop:
  \        MOV X10, #10
  \        UDIV X11, X9, X10
  \        MSUB X12, X11, X10, X9  (remainder)
  \        ADD X12, X12, #48 ('0')
  \        Push X12 to hardware stack
  \        MOV X9, X11
  \        CBNZ X9, loop
  \   7. Print loop: pop digit, write it
  \   8. Print space
  ... ;
```

### Test

```bash
# Test gen-emit: emit character 'A' (65)
# Compile: : main 65 emit bye ;
# EXPECTED output: A

# Test gen-cr: emit newline
# EXPECTED output: (blank line)

# Test gen-dot: print 42
# EXPECTED output: 42
```

**Phase 6 Exit Criteria:** `.` `cr` `emit` `type` produce correct output.
Test suite can now use output comparison instead of just exit codes.

---

## Phase 7: rstack.fs + Mixed Files

### rstack.fs

Direct port: replace RBP with X28, replace x86 instructions with ARM64.

```forth
: gen->r ( -- )
  \ SUB X28, X28, #8
  8 X28 sub-ri
  \ STR X19, [X28]
  X19 X28 str-r[r]
  pop-val ;

: gen-r> ( -- )
  push-val
  \ LDR X19, [X28]
  X19 X28 ldr-r[r]
  \ ADD X28, X28, #8
  8 X28 add-ri ;

: gen-r@ ( -- )
  push-val
  X19 X28 ldr-r[r] ;
```

### defs.fs Changes

The `$48 c, $b8 c, ... q, $c3 c,` pattern (mov rax,imm64; ret) becomes
MOVZ/MOVK X19, imm64; RET. The $800 inlining flag currently reads the imm64
value from code offset +2 (skipping the 48 b8 prefix). On ARM64, the value
is encoded across multiple MOVZ/MOVK instructions, making it impractical to
read back.

**Solution:** Store the constant value in a separate field of the dict entry,
or in a known location after the code. For the port, the simplest approach:
change the $800 inlining to store the value at compile time (it's already
available as `data-here @` or `const-val @`) rather than reading it back from
generated code.

### strings.fs Changes

The `$e9 c, ... 0 d,` (jmp rel32) becomes a 4-byte `B imm26`.
The `$48 c, $b8 c, ... q,` (mov rax, imm64) becomes MOVZ/MOVK X19.

The displacement calculation changes:
- x86: displacement = target - (patch_addr + 4)
- ARM64: offset = (target - instruction_addr) / 4, encoded in the instruction

### compile.fs Changes

Replace inline x86 bytes with ARM64 equivalents:

```
$48 c, $f7 c, $db c,   →  nos neg-r          \ NEG X20, X20
$48 c, $ff c, $c3 c,   →  nos inc-r          \ ADD X20, X20, #1
$48 c, $ff c, $cb c,   →  nos dec-r          \ SUB X20, X20, #1
1 tos sar-ri            →  1 tos asr-ri       \ ASR X19, X19, #1
$ff c, $d0 c,          →  tos emit-blr       \ BLR X19
```

### main.fs Changes

#### gen-prologue (ARM64 macOS)

```forth
: gen-prologue ( -- )
  \ LC_MAIN convention: dyld calls entrypoint like C main().
  \ argc is at [SP], argv is at [SP+8]. (NOT in registers.)
  \ Save argc: LDR X9, [SP]
  $F94003E9 emit32
  \ Store to rt-argc: need MOVZ/MOVK for address, then STR
  X10 mov-ri rt-argc
  X9 X10 str-r[r]
  \ Save argv: ADD X9, SP, #8
  $91002009 emit32    \ ADD X9, SP, #8 (= 2 << 10 | 9 | (31 << 5))
  \ Store to rt-argv
  X10 mov-ri rt-argv
  X9 X10 str-r[r]
  \ Set up data stack pointer: X22 = DATA-BASE + some offset
  X22 mov-ri DATA-BASE $100000 +   \ X22 → data stack area
  \ Set up return stack pointer: X28 = DATA-BASE + another offset
  X28 mov-ri DATA-BASE $180000 +   \ X28 → return stack area
  \ Call main (BL, patched later)
  $94000000 emit32
  code-here start-jmp ! ;

: gen-epilogue ( -- )
  0 0 X0 movz            \ MOV X0, #0
  1 0 X16 movz           \ MOV X16, #1
  emit-svc ;             \ SVC #0x80
```

#### compile-token call generation

Replace x86 push/pop/call with ARM64 STP/LDP/BL:

```forth
\ Instead of: $53 c, (push rbx) $51 c, (push rcx)
\ Use: STP X20, X21, [SP, #-16]!
X20 X21 31 -16 stp-pre

\ Instead of: $e8 c, d, (call rel32)
\ Use: BL offset (4 bytes, needs patching)
$94000000 emit32

\ Instead of: $59 c, (pop rcx) $5b c, (pop rbx)
\ Use: LDP X20, X21, [SP], #16
X20 X21 31 16 ldp-post
```

---

## Phase 8: Integration and main.fs

**Goal:** Wire everything together so the compiler produces runnable ARM64 Mach-O.

### Step 8.1: Switch Includes

Change main.fs from:
```forth
include compiler/shannon/arch/x86/asm.fs
include compiler/shannon/arch/x86/stack.fs
...
include compiler/shannon/arch/x86/elf.fs
```
To:
```forth
include compiler/shannon/arch/arm64/asm.fs
include compiler/shannon/arch/arm64/stack.fs
...
include compiler/shannon/arch/arm64/macho.fs
```

### Step 8.2: Update compile-file

```forth
: compile-file ( src-addr src-u out-addr out-u -- )
  2swap load-source
  scan-all
  0 input-pos !
  0 code-pos !
  0 dict-count !
  0 stack-depth !
  ct-reset
  clear-swap
  gen-prologue
  gen-epilogue
  compile-all
  patch-start
  \ ARM64: use macho instead of elf
  code-pos @ macho-header
  write-macho ;
```

### Step 8.3: Add Code Signing

After writing the binary, either:
- Automatically run `codesign -f -s -` from the test harness
- Or document that users must sign before running

The test runner should handle this.

### Step 8.4: Test End-to-End

```bash
# Compile a real Forth program with Shannon ARM64
./engine/fifth compiler/shannon/main.fs compiler/tests/01-lit.fs /tmp/t
chmod +x /tmp/t && codesign -f -s - /tmp/t
/tmp/t; echo $?
# Compare against expected output from the test
```

**Phase 8 Exit Criteria:** `./engine/fifth compiler/shannon/main.fs input.fs /tmp/t`
produces a runnable ARM64 Mach-O binary.

---

## Phase 9: Test Suite

### Step 9.1: Update Test Runner

Modify `compiler/tests/test` to:
1. Detect platform: `uname -m` → `arm64` on Apple Silicon
2. Add `codesign -f -s -` before execution
3. Keep the same test format (expect: comments)

### Step 9.2: Run Tests

```bash
./compiler/tests/test
# Initial goal: more tests passing than failing
# Final goal: same pass rate as x86 backend
```

Fix failures one at a time. Each failure is in a specific phase — use the
phase dependencies to narrow down where.

---

## Phase 10: Self-Hosting

Shannon compiles Shannon on ARM64. When this works, the C engine is scaffolding.

---

## Diagnostic Playbook

### "Killed: 9"

macOS killed the process. Almost always a code signing issue.
```bash
codesign -f -s - /tmp/binary    # Sign it
codesign -v /tmp/binary      # Verify signature
```

### "Bus error: 10"

Memory alignment violation. On ARM64, unaligned loads/stores to certain
address ranges cause bus errors. Check:
- Is DATA-BASE properly aligned?
- Is SP 16-byte aligned before SVC?
- Are LDR/STR offsets correct?

### "Segmentation fault: 11"

Bad memory access. Most common causes:
- Entry point wrong (check macho.fs entryoff in LC_MAIN)
- Stack pointer not set up (X22 or X28 pointing to unmapped memory)
- Code jumped to wrong address (check branch offset calculation)

Debug with lldb:
```bash
lldb /tmp/binary
(lldb) run
(lldb) register read
(lldb) disassemble --pc
(lldb) memory read $sp
```

### Wrong exit code

The code runs but produces wrong result. Debug steps:
1. `otool -tv /tmp/binary` — disassemble, verify instructions look right
2. Compare instruction bytes to expected encodings
3. Check operand order (ARM64 operand order differs from x86!)

### Binary is 0 bytes

File write failed. Check:
- Does the output path exist and is writable?
- Did `write-file` get the right buffer and count?
- Run with `VERBOSE=1` if available.

### "Malformed Mach-O file"

Header structure wrong. Steps:
1. `otool -l /tmp/binary` — check load commands
2. `xxd /tmp/binary | head -40` — check magic bytes
3. Diff against `tools/ref-exit42.hex` — find the byte difference
4. Check: cmdsize values sum correctly? ncmds matches?

---

## File Checklist

Update as each file is completed and tested:

```
PHASE  FILE                       STATUS  TEST
─────  ─────────────────────────  ──────  ────────────────────────
  0    tools/macho-test.c         [✓]     exit 42, arith, hello
  1    arch/arm64/asm.fs          [~]     byte-level encoding tests
  2    arch/arm64/macho.fs        [ ]     Forth-generated exit(42)
  3    arch/arm64/stack.fs        [ ]     push/pop/cascade via exit codes
  4    arch/arm64/prims.fs        [ ]     each emit-* via exit codes
  5    arch/arm64/control.fs      [ ]     if/then, do/loop via exit codes
  6    arch/arm64/io.fs           [ ]     emit, cr, dot output tests
  7a   arch/arm64/rstack.fs       [ ]     >r r> via exit codes
  7b   defs.fs                    [ ]     variable/constant tests
  7c   strings.fs                 [ ]     s" type tests
  7d   compile.fs                 [ ]     inline byte replacements
  8    main.fs                    [ ]     end-to-end compilation
  9    compiler/tests/test        [ ]     full test suite
  10   self-hosting               [ ]     Shannon compiles Shannon
```

[~] = stub exists, not complete

---

## Implementation Order Summary

```
Phase 0  ──→  Phase 1  ──→  Phase 2  ──→  Phase 3
(C tool)      (asm.fs)      (macho.fs)    (stack.fs)
                                              │
                    ┌─────────────────────────┤
                    ▼                         ▼
                Phase 4                   Phase 5
               (prims.fs)              (control.fs)
                    │                         │
                    ├─────────────────────────┤
                    ▼                         ▼
                Phase 6                   Phase 7
                (io.fs)              (rstack+mixed)
                    │                         │
                    └─────────┬───────────────┘
                              ▼
                          Phase 8
                         (main.fs)
                              │
                              ▼
                          Phase 9
                        (test suite)
                              │
                              ▼
                          Phase 10
                       (self-hosting)
```

Phases 0→1→2→3 are strictly sequential (each depends on the previous).
Phases 4, 5, 6, 7 can be done in any order after Phase 3.
Phase 8 requires all of 4-7 to be complete.

---

*Document version: 3.0 — Updated for LC_MAIN dynamic linking (LC_UNIXTHREAD
does not work on macOS ARM64). Phase 0 complete with tools/macho-test.c.
All codesign commands use `-f` flag. Written for humans who've been burned
by abstraction spirals.*

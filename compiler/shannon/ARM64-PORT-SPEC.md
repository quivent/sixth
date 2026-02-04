# Shannon ARM64 Port Specification

## Executive Summary

Port the Shannon Forth compiler from x86-64/ELF to ARM64. Target platform: macOS on Apple Silicon (M4 Max). Output format: Mach-O executable.

This document provides everything needed for humans and AI agents to execute the port.

---

## 1. Current Architecture (x86-64)

### 1.1 Register Mapping

Shannon uses a register-mapped stack for performance:

| Role | x86-64 Register | Purpose |
|------|-----------------|---------|
| TOS | RAX | Top of stack (most recently pushed) |
| NOS | RBX | Next on stack (second item) |
| 3rd | RCX | Third stack item |
| Stack Ptr | R15 | Points to memory stack for overflow |
| Return | RSP | Hardware return stack |

When the stack exceeds 3 items, values spill to memory at `[R15]`. The `stkptr` abstraction handles this.

### 1.2 Code Generation Model

Shannon emits raw bytes directly to a buffer:

```forth
: emit-dup ( -- )
  \ ( x -- x x ) duplicate TOS
  push-val              \ make room
  tos nos mov-rr ;      \ mov rbx, rax
```

The `mov-rr`, `add-ri8`, etc. words emit x86-64 machine code bytes. These are defined in `prims.fs`.

### 1.3 Key Files

| File | Purpose | Port Impact |
|------|---------|-------------|
| `prims.fs` | x86-64 instruction emitters | **REWRITE** |
| `codegen.fs` | Low-level byte emission, REX prefixes | **REWRITE** |
| `elf.fs` | ELF header generation | **REPLACE** with Mach-O |
| `compile.fs` | Word compilation, constant folding | Minimal changes |
| `control.fs` | Control flow (if/then, loops) | Update branch encoding |
| `dispatch.fs` | Builtin word table | No changes |
| `scan.fs` | Two-pass scanner | No changes |
| `main.fs` | Entry point, orchestration | Update addresses |

### 1.4 Memory Layout (x86-64 ELF)

```
0x400000  CODE-BASE   Text segment (code)
0x800000  DATA-BASE   Data segment (variables)
```

These are ELF virtual addresses. Mach-O uses different conventions.

---

## 2. ARM64 Architecture

### 2.1 Register Set

ARM64 has 31 general-purpose 64-bit registers:

| Register | Convention | Notes |
|----------|------------|-------|
| x0-x7 | Arguments/results | Caller-saved |
| x8 | Indirect result | Caller-saved |
| x9-x15 | Temporaries | Caller-saved |
| x16-x17 | Intra-procedure | Platform reserved |
| x18 | Platform register | Reserved on macOS |
| x19-x28 | Callee-saved | Preserved across calls |
| x29 | Frame pointer | FP |
| x30 | Link register | Return address (LR) |
| sp | Stack pointer | Hardware stack |

### 2.2 Proposed Register Mapping for Shannon

| Role | ARM64 Register | Rationale |
|------|----------------|-----------|
| TOS | x0 | Result register, fast access |
| NOS | x1 | Second argument register |
| 3rd | x2 | Third argument register |
| Stack Ptr | x19 | Callee-saved, survives calls |
| Scratch | x9, x10 | For complex operations |

**Alternative (callee-saved for all):**

| Role | ARM64 Register | Rationale |
|------|----------------|-----------|
| TOS | x19 | Preserved, no save/restore on calls |
| NOS | x20 | Preserved |
| 3rd | x21 | Preserved |
| Stack Ptr | x22 | Preserved |

The callee-saved approach is safer but means function calls preserve our stack automatically.

### 2.3 Instruction Encoding

ARM64 instructions are fixed 32-bit width. This is MUCH simpler than x86-64's variable-length encoding.

Key instruction formats:

```
ADD  Xd, Xn, Xm      ; Rd = Rn + Rm
ADD  Xd, Xn, #imm12  ; Rd = Rn + immediate (12-bit)
SUB  Xd, Xn, Xm      ; Rd = Rn - Rm
MOV  Xd, Xn          ; Rd = Rn (alias for ORR Xd, XZR, Xn)
MOV  Xd, #imm16      ; Rd = immediate (with shift)
LDR  Xd, [Xn]        ; Rd = mem[Xn]
STR  Xd, [Xn]        ; mem[Xn] = Rd
LDR  Xd, [Xn, #off]  ; Rd = mem[Xn + offset]
B    label           ; Unconditional branch
B.cond label         ; Conditional branch
BL   label           ; Branch with link (call)
RET                  ; Return (BR X30)
```

### 2.4 Immediates

ARM64 cannot load arbitrary 64-bit immediates in one instruction. Options:

1. **Small immediates (12-bit):** Use ADD/SUB with immediate
2. **16-bit chunks:** Use MOVZ/MOVK sequence
3. **PC-relative:** Use ADR/ADRP for addresses
4. **Literal pool:** Load from nearby memory

For loading a 64-bit constant like `0x800000`:
```asm
MOVZ  X0, #0x80, LSL #16    ; X0 = 0x800000
```

For arbitrary 64-bit values:
```asm
MOVZ  X0, #imm0             ; bits 0-15
MOVK  X0, #imm1, LSL #16    ; bits 16-31
MOVK  X0, #imm2, LSL #32    ; bits 32-47
MOVK  X0, #imm3, LSL #48    ; bits 48-63
```

### 2.5 Condition Codes

ARM64 condition codes (for B.cond):

| Code | Meaning | Flags |
|------|---------|-------|
| EQ | Equal | Z=1 |
| NE | Not equal | Z=0 |
| LT | Signed less than | N!=V |
| LE | Signed less or equal | Z=1 or N!=V |
| GT | Signed greater than | Z=0 and N=V |
| GE | Signed greater or equal | N=V |
| LO | Unsigned lower | C=0 |
| HI | Unsigned higher | C=1 and Z=0 |

Use CMP before conditional branch:
```asm
CMP   X0, X1        ; Compare, set flags
B.LT  target        ; Branch if X0 < X1 (signed)
```

---

## 3. Mach-O Executable Format

### 3.1 Overview

macOS uses Mach-O instead of ELF. Key differences:

| Aspect | ELF | Mach-O |
|--------|-----|--------|
| Magic | 0x7F 'E' 'L' 'F' | 0xFEEDFACF (64-bit) |
| Sections | Segments + sections | Load commands + segments + sections |
| Entry | e_entry field | LC_MAIN load command |
| Code address | 0x400000 typical | 0x100000000 typical |
| Data address | 0x800000 typical | Adjacent to code |

### 3.2 Minimal Mach-O Structure

```
+------------------+
| Mach-O Header    |  32 bytes
+------------------+
| Load Commands    |  Variable
|  - LC_SEGMENT_64 |  __TEXT segment
|  - LC_SEGMENT_64 |  __DATA segment
|  - LC_MAIN       |  Entry point
|  - LC_DYLD_INFO  |  (if dynamic linking)
+------------------+
| __TEXT Segment   |
|  - __text section|  Code goes here
+------------------+
| __DATA Segment   |
|  - __data section|  Variables here
+------------------+
```

### 3.3 Mach-O Header (64-bit)

```c
struct mach_header_64 {
    uint32_t magic;         // 0xFEEDFACF
    uint32_t cputype;       // CPU_TYPE_ARM64 = 0x0100000C
    uint32_t cpusubtype;    // CPU_SUBTYPE_ARM64_ALL = 0
    uint32_t filetype;      // MH_EXECUTE = 2
    uint32_t ncmds;         // Number of load commands
    uint32_t sizeofcmds;    // Size of all load commands
    uint32_t flags;         // MH_NOUNDEFS | MH_PIE typically
    uint32_t reserved;      // 0
};
```

### 3.4 Key Constants

```forth
$FEEDFACF constant MH_MAGIC_64
$0100000C constant CPU_TYPE_ARM64
$00000000 constant CPU_SUBTYPE_ARM64_ALL
$00000002 constant MH_EXECUTE

\ Load command types
$19 constant LC_SEGMENT_64
$80000028 constant LC_MAIN

\ Segment/section flags
$80000000 constant S_ATTR_PURE_INSTRUCTIONS
$00000400 constant S_ATTR_SOME_INSTRUCTIONS

\ Typical base address for macOS ARM64
$100000000 constant CODE-BASE
```

### 3.5 Minimal Static Executable

For a minimal static executable (no dyld):
1. Mach-O header
2. LC_SEGMENT_64 for __TEXT (code)
3. LC_SEGMENT_64 for __DATA (if needed)
4. LC_UNIXTHREAD (sets initial register state including PC)

Note: LC_MAIN requires dyld. For truly static, use LC_UNIXTHREAD.

---

## 4. Port Strategy

### 4.1 Phase 1: ARM64 Instruction Emitters

Create `arm64.fs` to replace x86-64 codegen:

```forth
\ arm64.fs - ARM64 instruction emitters

: emit32 ( u -- )
  \ Emit 32-bit little-endian instruction
  dup         c,
  8 rshift dup c,
  8 rshift dup c,
  8 rshift     c, ;

\ Register encoding (0-30, 31=SP/ZR)
: reg ( n -- n ) ; \ identity, for clarity

\ ADD Xd, Xn, Xm
: add-rrr ( d n m -- )
  \ 10001011000 Rm 000000 Rn Rd
  5 lshift swap 16 lshift or swap 21 lshift or
  $8B000000 or emit32 ;

\ SUB Xd, Xn, Xm
: sub-rrr ( d n m -- )
  5 lshift swap 16 lshift or swap 21 lshift or
  $CB000000 or emit32 ;

\ MOV Xd, Xn (ORR Xd, XZR, Xn)
: mov-rr ( d n -- )
  31 swap add-rrr drop \ ORR Xd, XZR, Xn = ADD Xd, XZR, Xn...
  \ Actually: ORR is different encoding
  \ MOV Xd, Xn = ORR Xd, XZR, Xn
  \ 10101010000 Xm 000000 11111 Xd
  16 lshift swap or $AA0003E0 or emit32 ;

\ LDR Xd, [Xn]
: ldr-r[r] ( d n -- )
  \ 11111000010 000000000 00 Xn Xd
  5 lshift swap or $F8400000 or emit32 ;

\ STR Xd, [Xn]
: str-r[r] ( d n -- )
  5 lshift swap or $F8000000 or emit32 ;

\ RET (BR X30)
: emit-ret ( -- )
  $D65F03C0 emit32 ;

\ B (unconditional branch)
: emit-b ( offset -- )
  \ offset is in instructions (multiply by 4 after)
  4 / $3FFFFFF and $14000000 or emit32 ;

\ BL (branch with link = call)
: emit-bl ( offset -- )
  4 / $3FFFFFF and $94000000 or emit32 ;
```

### 4.2 Phase 2: Adapt prims.fs

Map each `emit-*` word to ARM64 equivalent:

```forth
\ x86-64 version:
: emit-+ ( -- )
  nos tos add-rr    \ add rax, rbx
  pop-nos-val ;

\ ARM64 version:
: emit-+ ( -- )
  tos tos nos add-rrr   \ add x0, x0, x1
  pop-nos-val ;
```

The stack management (`push-val`, `pop-val`, etc.) stays conceptually the same, just with different registers.

### 4.3 Phase 3: Replace elf.fs with macho.fs

Create `macho.fs`:

```forth
\ macho.fs - Mach-O executable generation for ARM64 macOS

$FEEDFACF constant MH_MAGIC_64
$0100000C constant CPU_TYPE_ARM64
$00000002 constant MH_EXECUTE
$19       constant LC_SEGMENT_64
$80000028 constant LC_MAIN

: macho-header ( -- )
  MH_MAGIC_64 ,32          \ magic
  CPU_TYPE_ARM64 ,32       \ cputype
  0 ,32                    \ cpusubtype
  MH_EXECUTE ,32           \ filetype
  3 ,32                    \ ncmds (adjust as needed)
  ... ;
```

### 4.4 Phase 4: Update Constants

In `main.fs` or new `config.fs`:

```forth
\ ARM64 macOS addresses
$100000000 constant CODE-BASE   \ __TEXT segment base
$100010000 constant DATA-BASE   \ __DATA segment base (example)
```

---

## 5. Instruction Translation Reference

### 5.1 Stack Operations

| Operation | x86-64 | ARM64 |
|-----------|--------|-------|
| DUP | `push rbx; mov rbx,rax` | `str x1,[x19,#-8]!; mov x1,x0` |
| DROP | `mov rax,rbx; pop rbx` | `mov x0,x1; ldr x1,[x19],#8` |
| SWAP | `xchg rax,rbx` | `mov x9,x0; mov x0,x1; mov x1,x9` |
| OVER | `push rbx; mov rbx,rax; (get 3rd)` | Similar with ARM regs |

### 5.2 Arithmetic

| Operation | x86-64 | ARM64 |
|-----------|--------|-------|
| + | `add rax,rbx` | `add x0,x0,x1` |
| - | `sub rax,rbx` | `sub x0,x1,x0` (note order!) |
| * | `imul rax,rbx` | `mul x0,x0,x1` |
| / | `cqo; idiv rbx` | `sdiv x0,x1,x0` |
| AND | `and rax,rbx` | `and x0,x0,x1` |
| OR | `or rax,rbx` | `orr x0,x0,x1` |
| XOR | `xor rax,rbx` | `eor x0,x0,x1` |
| NEGATE | `neg rax` | `neg x0,x0` |
| INVERT | `not rax` | `mvn x0,x0` |

### 5.3 Memory

| Operation | x86-64 | ARM64 |
|-----------|--------|-------|
| @ | `mov rax,[rax]` | `ldr x0,[x0]` |
| ! | `mov [rax],rbx` | `str x1,[x0]` |
| C@ | `movzx rax,byte[rax]` | `ldrb w0,[x0]` |
| C! | `mov [rax],bl` | `strb w1,[x0]` |

### 5.4 Control Flow

| Operation | x86-64 | ARM64 |
|-----------|--------|-------|
| Jump | `jmp rel32` | `b offset` |
| Call | `call rel32` | `bl offset` |
| Return | `ret` | `ret` (BR X30) |
| If zero | `test rax,rax; jz` | `cbz x0,offset` |
| If nonzero | `test rax,rax; jnz` | `cbnz x0,offset` |
| Compare | `cmp rax,rbx` | `cmp x0,x1` |
| Branch < | `jl offset` | `b.lt offset` |
| Branch = | `je offset` | `b.eq offset` |

### 5.5 Literals

Loading immediate values:

```forth
\ x86-64: mov rax, imm64 (10 bytes)
: emit-lit ( n -- )
  $48 c, $B8 c, ,64 ;  \ movabs rax, imm64

\ ARM64: MOVZ/MOVK sequence (4-16 bytes)
: emit-lit ( n -- )
  dup $FFFF and
  $D2800000 or           \ MOVZ X0, #imm16
  tos 0 lshift or emit32

  dup 16 rshift $FFFF and ?dup if
    $F2A00000 or tos 0 lshift or emit32  \ MOVK X0, #imm16, LSL #16
  then
  \ ... continue for bits 32-47, 48-63 if nonzero
  drop ;
```

---

## 6. System Calls (macOS ARM64)

### 6.1 Calling Convention

macOS ARM64 system calls:
- Syscall number in X16
- Arguments in X0-X5
- Return value in X0
- Use SVC #0x80 instruction

```forth
: emit-syscall ( -- )
  $D4001001 emit32 ;   \ SVC #0x80

: emit-exit ( -- )
  \ exit(status) - status in X0
  1 emit-lit          \ syscall number for exit
  16 0 mov-rr         \ mov x16, x0... wait, need to reorganize
  \ X0 = exit code, X16 = syscall number
  $D2800030 emit32    \ MOVZ X16, #1 (exit syscall)
  $D4001001 emit32 ;  \ SVC #0x80
```

### 6.2 Key Syscall Numbers (macOS ARM64)

| Syscall | Number | Arguments |
|---------|--------|-----------|
| exit | 1 | X0=status |
| write | 4 | X0=fd, X1=buf, X2=len |
| read | 3 | X0=fd, X1=buf, X2=len |
| open | 5 | X0=path, X1=flags, X2=mode |
| close | 6 | X0=fd |

---

## 7. Testing Strategy

### 7.1 Minimal Test

First working program:

```forth
: main 42 . cr ;
```

Should:
1. Load constant 42
2. Call print routine
3. Print newline
4. Exit cleanly

### 7.2 Progressive Tests

1. **Exit code:** `42 bye` - verify exit syscall works
2. **Arithmetic:** `2 3 + .` - verify stack and math
3. **Memory:** `variable x  5 x !  x @ .` - verify load/store
4. **Control:** `10 0 do i . loop` - verify loops
5. **Self-host:** Compile Shannon with Shannon

### 7.3 Debugging Tools

```bash
# Disassemble Mach-O
otool -tv output

# Show load commands
otool -l output

# Debug with lldb
lldb ./output
(lldb) disassemble
(lldb) register read
```

---

## 8. Files to Create/Modify

### 8.1 New Files

| File | Purpose |
|------|---------|
| `arm64.fs` | ARM64 instruction encoding |
| `macho.fs` | Mach-O executable generation |
| `syscall-macos.fs` | macOS system call wrappers |

### 8.2 Modified Files

| File | Changes |
|------|---------|
| `prims.fs` | Replace x86-64 emitters with ARM64 |
| `codegen.fs` | Remove REX prefix logic, update for 32-bit fixed instructions |
| `control.fs` | Update branch offset calculations (ARM64 branches are instruction-relative, not byte-relative) |
| `main.fs` | Update CODE-BASE, DATA-BASE, include new files |
| `compile.fs` | Minimal changes (stack tracking stays same) |

### 8.3 Unchanged Files

| File | Why |
|------|-----|
| `dispatch.fs` | Pure data table |
| `scan.fs` | Parsing, no codegen |
| `strings.fs` | High-level string ops |
| `defs.fs` | Variable/constant compilation (may need address updates) |

---

## 9. Risks and Mitigations

### 9.1 Immediate Encoding Complexity

**Risk:** ARM64 can't encode arbitrary immediates.

**Mitigation:** Always emit MOVZ/MOVK sequence for safety. Optimize later.

### 9.2 Branch Range

**Risk:** ARM64 branches have limited range (B.cond: +/-1MB, B: +/-128MB).

**Mitigation:** For now, assume code fits. Add veneer/trampoline support later if needed.

### 9.3 Code Signing

**Risk:** macOS requires code signing to run executables.

**Mitigation:** Use `codesign -s - output` to ad-hoc sign, or disable SIP for testing.

### 9.4 Stack Alignment

**Risk:** macOS requires 16-byte stack alignment at calls.

**Mitigation:** Ensure stack pointer is aligned before BL instructions.

---

## 10. Quick Reference Card

### Registers
```
TOS = X0    NOS = X1    3rd = X2    StkPtr = X19
Scratch = X9, X10       Link = X30 (LR)
```

### Common Instructions
```
ADD  Xd, Xn, Xm     ; Rd = Rn + Rm
SUB  Xd, Xn, Xm     ; Rd = Rn - Rm
MOV  Xd, Xn         ; Rd = Rn
MOVZ Xd, #imm       ; Rd = imm (zero others)
MOVK Xd, #imm, LSL  ; Rd[bits] = imm (keep others)
LDR  Xd, [Xn]       ; Rd = mem[Xn]
STR  Xd, [Xn]       ; mem[Xn] = Rd
B    offset         ; branch unconditional
BL   offset         ; branch with link (call)
RET                 ; return (BR X30)
CBZ  Xn, offset     ; branch if Xn == 0
CBNZ Xn, offset     ; branch if Xn != 0
CMP  Xn, Xm         ; set flags for Xn - Xm
B.EQ/NE/LT/GT/LE/GE ; conditional branch
SVC  #0x80          ; syscall
```

### Instruction Encodings (hex bases)
```
ADD  (reg):  0x8B000000
SUB  (reg):  0xCB000000
ORR  (mov):  0xAA0003E0  (MOV Xd, Xm)
MOVZ:        0xD2800000
MOVK:        0xF2800000
LDR  (reg):  0xF8400000
STR  (reg):  0xF8000000
B:           0x14000000
BL:          0x94000000
RET:         0xD65F03C0
CBZ:         0xB4000000
CBNZ:        0xB5000000
SVC #0x80:   0xD4001001
```

---

## 11. First Steps

1. **Create `arm64.fs`** with basic instruction emitters
2. **Create `macho.fs`** with minimal header generation
3. **Modify `prims.fs`** to use ARM64 emitters
4. **Test:** Emit a program that just exits with code 42
5. **Iterate:** Add arithmetic, memory, control flow
6. **Self-host:** Once complete, compile Shannon with Shannon

---

## Appendix A: ARM64 Instruction Encoding Details

### A.1 Data Processing (Register)

```
31 30 29 28 27 26 25 24 23 22 21 20-16 15-10  9-5  4-0
sf  0  0  1  0  1  0  1  sh  0  Rm    imm6   Rn   Rd   ADD
sf  1  0  1  0  1  0  1  sh  0  Rm    imm6   Rn   Rd   SUB
```

sf=1 for 64-bit (X registers), sf=0 for 32-bit (W registers).

### A.2 Loads and Stores

```
31-30 29-27 26 25-24 23-22 21  20-16  15-13 12 11-10 9-5  4-0
 size 111   0  00    opc   0   Rm     opt   S  10    Rn   Rt   LDR/STR (register)
 size 111   0  01    opc   imm9           01        Rn   Rt   LDR/STR (post-index)
 size 111   0  01    opc   imm9           11        Rn   Rt   LDR/STR (pre-index)
 size 111   0  01    opc        imm12              Rn   Rt   LDR/STR (unsigned offset)
```

### A.3 Branches

```
31 30-26  25-0
0  00101  imm26                    B (unconditional)
1  00101  imm26                    BL (call)

31 30-25  24 23-5   4-0
0  011010 0  imm19  cond           B.cond

31 30-25  24 23-5   4-0
x  011010 0  imm19  Rt             CBZ/CBNZ
```

---

## Appendix B: Mach-O Load Command Structures

### B.1 segment_command_64

```c
struct segment_command_64 {
    uint32_t cmd;           // LC_SEGMENT_64 = 0x19
    uint32_t cmdsize;       // sizeof + sections
    char     segname[16];   // "__TEXT", "__DATA", etc.
    uint64_t vmaddr;        // Virtual address
    uint64_t vmsize;        // Virtual size
    uint64_t fileoff;       // File offset
    uint64_t filesize;      // File size
    uint32_t maxprot;       // Max VM protection
    uint32_t initprot;      // Initial VM protection
    uint32_t nsects;        // Number of sections
    uint32_t flags;         // Segment flags
};
```

### B.2 section_64

```c
struct section_64 {
    char     sectname[16];  // "__text", "__data", etc.
    char     segname[16];   // Parent segment name
    uint64_t addr;          // Virtual address
    uint64_t size;          // Size in bytes
    uint32_t offset;        // File offset
    uint32_t align;         // Alignment (power of 2)
    uint32_t reloff;        // Relocation offset
    uint32_t nreloc;        // Number of relocations
    uint32_t flags;         // Section type and attributes
    uint32_t reserved1;
    uint32_t reserved2;
    uint32_t reserved3;
};
```

### B.3 entry_point_command (LC_MAIN)

```c
struct entry_point_command {
    uint32_t cmd;           // LC_MAIN = 0x80000028
    uint32_t cmdsize;       // 24
    uint64_t entryoff;      // Offset from __TEXT start
    uint64_t stacksize;     // Initial stack size (0 = default)
};
```

---

*Document version: 1.0*
*Created for Shannon ARM64 port*
*Target: macOS Apple Silicon (M4 Max)*

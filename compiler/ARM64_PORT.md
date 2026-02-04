# ARM64 macOS Port Specification

## Overview

Port sixth from x86-64 Linux (ELF) to ARM64 macOS (Mach-O). The compiler architecture remains identical — only the code generation layer changes.

## Scope

| Component | Action |
|-----------|--------|
| Parsing, input handling | Reuse as-is |
| Two-pass infrastructure | Reuse as-is |
| Constant folding | Reuse as-is |
| Control flow compilation | Reuse as-is |
| Optimization logic | Reuse as-is |
| `gen-*` primitives (~170 functions) | **Rewrite** |
| ELF header | **Replace with Mach-O** |
| Post-build signing | **Add** |

## Register Mapping

### x86-64 Current Convention
```
rax = TOS (top of stack)
rbx = NOS (next on stack)
rcx = 3rd element
r15 = data stack pointer (grows down)
rbp = return stack pointer
r12 = DO/LOOP index
r13 = DO/LOOP limit
```

### ARM64 Proposed Convention
```
x0  = TOS (top of stack)
x1  = NOS (next on stack)
x2  = 3rd element
x20 = data stack pointer (grows down)
x21 = return stack pointer
x22 = DO/LOOP index
x23 = DO/LOOP limit
x16 = syscall number (per Apple ABI)
x17 = scratch (per Apple ABI)
```

**Rationale:**
- x0-x2: Argument registers, convenient for TOS/NOS
- x20-x23: Callee-saved, persist across calls
- x16/x17: Reserved by Apple for syscalls and linker

## Instruction Mapping

### Arithmetic

| x86-64 | ARM64 | Notes |
|--------|-------|-------|
| `add rax, rbx` | `add x0, x0, x1` | |
| `sub rax, rbx` | `sub x0, x0, x1` | |
| `imul rax, rbx` | `mul x0, x0, x1` | |
| `idiv` | `sdiv x0, x0, x1` | No remainder; use `msub` for mod |
| `neg rax` | `neg x0, x0` | |
| `inc rax` | `add x0, x0, #1` | |
| `dec rax` | `sub x0, x0, #1` | |

### Stack Operations

| Operation | x86-64 | ARM64 |
|-----------|--------|-------|
| push-tos | `sub r15,8; mov [r15],rbx; mov rbx,rax` | `str x1,[x20,#-8]!; mov x1,x0` |
| pop-tos | `mov rax,rbx; mov rbx,[r15]; add r15,8` | `mov x0,x1; ldr x1,[x20],#8` |
| gen-dup | push-tos | push-tos |
| gen-drop | pop-tos | pop-tos |
| gen-swap | `xchg rax, rbx` | `mov x17,x0; mov x0,x1; mov x1,x17` |

### Literals

| x86-64 | ARM64 | Notes |
|--------|-------|-------|
| `mov rax, imm32` | `mov x0, #imm16` | 16-bit immediate |
| `movabs rax, imm64` | `movz`+`movk` sequence | 4 instructions for 64-bit |

ARM64 literal loading (64-bit):
```asm
movz x0, #(val & 0xFFFF)
movk x0, #((val >> 16) & 0xFFFF), lsl #16
movk x0, #((val >> 32) & 0xFFFF), lsl #32
movk x0, #((val >> 48) & 0xFFFF), lsl #48
```

Optimize for common cases:
- Zero: `mov x0, xzr` (1 instruction)
- Small positive: `mov x0, #imm` (1 instruction, 16-bit)
- 32-bit: `movz` + `movk` (2 instructions)

### Comparisons and Branches

| x86-64 | ARM64 |
|--------|-------|
| `cmp rax, rbx` | `cmp x0, x1` |
| `test rax, rax` | `tst x0, x0` or `cmp x0, #0` |
| `je label` | `b.eq label` |
| `jne label` | `b.ne label` |
| `jl label` | `b.lt label` |
| `jg label` | `b.gt label` |
| `jmp label` | `b label` |

### Memory Access

| x86-64 | ARM64 |
|--------|-------|
| `mov rax, [rbx]` | `ldr x0, [x1]` |
| `mov [rbx], rax` | `str x0, [x1]` |
| `mov al, [rbx]` | `ldrb w0, [x1]` |
| `mov [rbx], al` | `strb w0, [x1]` |

### Calls and Returns

| x86-64 | ARM64 |
|--------|-------|
| `call rel32` | `bl label` |
| `ret` | `ret` |

ARM64 `bl` range is ±128MB. For larger programs, use:
```asm
adr x17, label
blr x17
```

### Syscalls

**x86-64 Linux:**
```asm
mov rax, syscall_num
mov rdi, arg1
mov rsi, arg2
mov rdx, arg3
syscall
```

**ARM64 macOS:**
```asm
mov x16, syscall_num
mov x0, arg1
mov x1, arg2
mov x2, arg3
svc #0x80
```

Syscall numbers differ between Linux and macOS:

| Syscall | Linux x86-64 | macOS ARM64 |
|---------|--------------|-------------|
| exit | 60 | 1 |
| read | 0 | 3 |
| write | 1 | 4 |
| open | 2 | 5 |
| close | 3 | 6 |

## Mach-O Header

Replace `elf-header` with Mach-O 64-bit header:

```
Offset  Size  Field
0x00    4     magic (0xFEEDFACF)
0x04    4     cputype (0x0100000C = ARM64)
0x08    4     cpusubtype (0x00000000)
0x0C    4     filetype (0x02 = MH_EXECUTE)
0x10    4     ncmds (number of load commands)
0x14    4     sizeofcmds
0x18    4     flags (MH_NOUNDEFS | MH_PIE)
0x1C    4     reserved (0)
```

Required load commands:
1. `LC_SEGMENT_64` for `__PAGEZERO` (guard page)
2. `LC_SEGMENT_64` for `__TEXT` (code)
3. `LC_SEGMENT_64` for `__DATA` (variables)
4. `LC_MAIN` (entry point)

Minimum file size: 16KB (ARM64 page size on macOS)

## Code Signing

After writing the binary:
```bash
codesign -s - /path/to/output
```

This creates an ad-hoc signature (no identity required). The compiler should either:
1. Shell out to `codesign` after writing the file
2. Emit the signature inline (more complex, not recommended initially)

## Implementation Strategy

### Phase 1: Parallel Backend
Create `sixth-arm.fs` alongside `sixth.fs`:
- Copy structure, replace `gen-*` bodies
- Separate file avoids merge conflicts
- Can test independently

### Phase 2: Mach-O Header
Implement `macho-header` word:
- Emit 64-bit Mach-O header
- `__TEXT` segment with executable code
- `__DATA` segment for variables
- Entry point via `LC_MAIN`

### Phase 3: Syscall Translation
Map Linux syscalls to macOS equivalents:
- Different numbers
- Slightly different semantics (error returns)

### Phase 4: Testing
1. Minimal "exit 42" program
2. Hello world (write syscall)
3. Arithmetic tests
4. Control flow tests
5. Full test suite

### Phase 5: Code Signing Integration
Add post-compile signing step.

## File Structure

```
compiler/
├── sixth.fs           # x86-64 Linux (existing)
├── sixth-arm.fs       # ARM64 macOS (new)
├── shared/            # Optional: extract shared code
│   ├── parse.fs
│   ├── optimize.fs
│   └── twopass.fs
└── ARM64_PORT.md      # This file
```

## Testing on x86-64 Machine

Cross-development options:
1. Develop on x86-64, test on Mac via SSH/rsync
2. Use QEMU user-mode emulation (slow)
3. Develop directly on Mac

Recommended: Develop on Mac for faster iteration.

## Risks and Mitigations

| Risk | Mitigation |
|------|------------|
| Branch range limits (±128MB) | Use indirect branches for large programs |
| Mach-O complexity | Start minimal, add features as needed |
| Syscall differences | Create abstraction layer |
| Code signing | Shell out to `codesign` initially |

## Estimated Effort

| Task | Effort |
|------|--------|
| Register mapping + stack ops | 2-4 hours |
| Arithmetic gen-* functions | 2-4 hours |
| Comparison/branch gen-* | 2-4 hours |
| Memory access gen-* | 1-2 hours |
| Mach-O header | 4-8 hours |
| Syscall translation | 2-4 hours |
| Testing + debugging | 8-16 hours |
| **Total** | **2-4 days** |

## References

- [ARM64 Instruction Set](https://developer.arm.com/documentation/ddi0596/latest)
- [Apple ARM64 ABI](https://developer.apple.com/documentation/xcode/writing-arm64-code-for-apple-platforms)
- [Mach-O Format](https://github.com/aidansteele/osx-abi-macho-file-format-reference)
- [macOS Syscalls](https://opensource.apple.com/source/xnu/xnu-7195.81.3/bsd/kern/syscalls.master)

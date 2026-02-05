\ asm.fs - ARM64 (AArch64) assembler abstraction (Shannon Layer 0)
\ Provides named words for ARM64 instruction emission.
\ Assumes: code-buf, code-pos, c,, d, defined externally.
\
\ ARM64 instructions are fixed 32-bit width. Much simpler than x86-64.
\ All instructions emitted via emit32 (little-endian 32-bit word).
\
\ STATUS: STUB - register constants and emit32 only.
\ Port one instruction at a time from arch/x86/asm.fs.

\ ============================================================
\ REGISTER CONSTANTS
\ ============================================================
\ ARM64 register encoding (5-bit values, 0-30, 31=SP/ZR)

0 constant X0    1 constant X1    2 constant X2    3 constant X3
4 constant X4    5 constant X5    6 constant X6    7 constant X7
8 constant X8    9 constant X9   10 constant X10  11 constant X11
12 constant X12  13 constant X13  14 constant X14  15 constant X15
16 constant X16  17 constant X17  18 constant X18  19 constant X19
20 constant X20  21 constant X21  22 constant X22  23 constant X23
24 constant X24  25 constant X25  26 constant X26  27 constant X27
28 constant X28  29 constant X29  30 constant X30  31 constant XZR

\ Named aliases
X29 constant FP      \ Frame pointer
X30 constant LR      \ Link register
XZR constant SP      \ Stack pointer (same encoding as XZR, context-dependent)

\ ============================================================
\ 32-BIT INSTRUCTION EMISSION
\ ============================================================

: emit32 ( u -- )
  \ Emit 32-bit little-endian ARM64 instruction
  dup         c,
  8 rshift dup c,
  8 rshift dup c,
  8 rshift     c, ;

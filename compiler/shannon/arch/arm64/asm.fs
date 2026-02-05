\ asm.fs - ARM64 instruction encoding + code buffer (Shannon Layer 0)
\ Clean-room implementation for ARM64/Mach-O compiler.
\ No x86 dependencies. All encodings verified against ARM Architecture Reference.
\
\ Provides:
\   Code buffer: code-buf, code-pos, >code, emit32
\   Instruction encoders: arm-movz, arm-movk, arm-svc, arm-add-reg, etc.

\ ============================================================
\ CODE BUFFER
\ ============================================================

262144 constant CODE-SIZE
create code-buf CODE-SIZE allot
variable code-pos   0 code-pos !

: >code ( b -- ) code-buf code-pos @ + c!  1 code-pos +! ;

: emit32 ( u -- )
  dup >code 8 rshift dup >code 8 rshift dup >code 8 rshift >code ;

\ ============================================================
\ IMMEDIATE MOVE INSTRUCTIONS
\ ============================================================

: arm-movz ( rd imm16 shift -- insn )
  16 / 21 lshift swap 5 lshift or or $D2800000 or ;

: arm-movk ( rd imm16 shift -- insn )
  16 / 21 lshift swap 5 lshift or or $F2800000 or ;

\ ============================================================
\ SUPERVISOR CALL
\ ============================================================

: arm-svc ( imm16 -- insn )
  5 lshift $D4000001 or ;

\ ============================================================
\ REGISTER-REGISTER OPERATIONS
\ ============================================================

\ All: ( rd rn rm -- insn )
: arm-add-reg ( rd rn rm -- insn )  16 lshift swap 5 lshift or or $8B000000 or ;
: arm-sub-reg ( rd rn rm -- insn )  16 lshift swap 5 lshift or or $CB000000 or ;
: arm-and-reg ( rd rn rm -- insn )  16 lshift swap 5 lshift or or $8A000000 or ;
: arm-orr-reg ( rd rn rm -- insn )  16 lshift swap 5 lshift or or $AA000000 or ;
: arm-eor-reg ( rd rn rm -- insn )  16 lshift swap 5 lshift or or $CA000000 or ;

\ Multiply: MUL Xd,Xn,Xm = MADD Xd,Xn,Xm,XZR
: arm-mul ( rd rn rm -- insn )      16 lshift swap 5 lshift or or $9B007C00 or ;

\ Signed divide: SDIV Xd,Xn,Xm
: arm-sdiv ( rd rn rm -- insn )     16 lshift swap 5 lshift or or $9AC00C00 or ;

\ Multiply-subtract: MSUB Xd,Xn,Xm,Xa  =>  Rd = Ra - Rn*Rm
: arm-msub ( rd rn rm ra -- insn )
  10 lshift swap 16 lshift or swap 5 lshift or swap or $9B008000 or ;

\ Variable shifts: ( rd rn rm -- insn )
: arm-lslv ( rd rn rm -- insn )     16 lshift swap 5 lshift or or $9AC02000 or ;
: arm-lsrv ( rd rn rm -- insn )     16 lshift swap 5 lshift or or $9AC02400 or ;
: arm-asrv ( rd rn rm -- insn )     16 lshift swap 5 lshift or or $9AC02800 or ;

\ ============================================================
\ IMMEDIATE ARITHMETIC
\ ============================================================

\ All: ( rd rn imm12 -- insn )
: arm-add-imm ( rd rn imm12 -- insn )  10 lshift swap 5 lshift or or $91000000 or ;
: arm-sub-imm ( rd rn imm12 -- insn )  10 lshift swap 5 lshift or or $D1000000 or ;

\ ============================================================
\ MOVE / BITWISE NOT HELPERS
\ ============================================================

\ MOV Xd, Xm = ORR Xd, XZR, Xm
: arm-mov-reg ( rd rm -- insn )
  16 lshift swap or 31 5 lshift or $AA000000 or ;

\ MVN Xd, Xm = ORN Xd, XZR, Xm
: arm-mvn ( rd rm -- insn )
  16 lshift swap or 31 5 lshift or $AA200000 or ;

\ ============================================================
\ LOAD / STORE WITH PRE/POST INDEX
\ ============================================================

\ STR Xt, [Xn, #imm9]!  (pre-index, 64-bit)
: arm-str-pre ( rt rn imm9 -- insn )
  $1FF and 12 lshift swap 5 lshift or swap or $F8000C00 or ;

\ LDR Xt, [Xn], #imm9  (post-index, 64-bit)
: arm-ldr-post ( rt rn imm9 -- insn )
  $1FF and 12 lshift swap 5 lshift or swap or $F8400400 or ;

\ LDR Xt, [Xn, #imm12*8]  (unsigned offset, 64-bit, scaled by 8)
: arm-ldr-off ( rt rn imm12 -- insn )
  10 lshift swap 5 lshift or swap or $F9400000 or ;

\ STR Xt, [Xn, #imm12*8]  (unsigned offset, 64-bit, scaled by 8)
: arm-str-off ( rt rn imm12 -- insn )
  10 lshift swap 5 lshift or swap or $F9000000 or ;

\ LDRB Wt, [Xn, #imm12]  (unsigned offset, byte, unscaled)
: arm-ldrb-off ( rt rn imm12 -- insn )
  10 lshift swap 5 lshift or swap or $39400000 or ;

\ STRB Wt, [Xn, #imm12]  (unsigned offset, byte, unscaled)
: arm-strb-off ( rt rn imm12 -- insn )
  10 lshift swap 5 lshift or swap or $39000000 or ;

\ ============================================================
\ COMPARE INSTRUCTIONS
\ ============================================================

\ CMP Xn, Xm = SUBS XZR, Xn, Xm
: arm-cmp-reg ( rn rm -- insn )
  16 lshift swap 5 lshift or 31 or $EB000000 or ;

\ CMP Xn, #imm12 = SUBS XZR, Xn, #imm12
: arm-cmp-imm ( rn imm12 -- insn )
  10 lshift swap 5 lshift or 31 or $F1000000 or ;

\ CSET Xd, cond = CSINC Xd, XZR, XZR, invert(cond)
\ Condition codes: EQ=0, NE=1, LT=11, GE=10, GT=12, LE=13
: arm-cset ( rd cond -- insn )
  1 xor 12 lshift swap or $9A9F07E0 or ;

\ ============================================================
\ CONDITIONAL BRANCHES (B.cond)
\ ============================================================

\ B.cond offset (offset in instructions, signed 19-bit)
: arm-bcond ( cond offset19 -- insn )
  $7FFFF and 5 lshift swap or $54000000 or ;

\ B unconditional branch (offset in instructions, signed 26-bit)
: arm-b ( offset26 -- insn )
  $3FFFFFF and $14000000 or ;

\ BL branch with link (offset in instructions, signed 26-bit)
: arm-bl ( offset26 -- insn )
  $3FFFFFF and $94000000 or ;

\ ============================================================
\ TEST AND CONDITIONAL SET
\ ============================================================

\ TST Xn, Xm = ANDS XZR, Xn, Xm
: arm-tst-reg ( rn rm -- insn )
  16 lshift swap 5 lshift or 31 or $EA000000 or ;

\ NEG Xd, Xm = SUB Xd, XZR, Xm
: arm-neg ( rd rm -- insn )
  16 lshift swap or 31 5 lshift or $CB000000 or ;

\ ASR Xd, Xn, #imm6 (arithmetic shift right immediate)
: arm-asr-imm ( rd rn imm6 -- insn )
  16 lshift 63 10 lshift or swap 5 lshift or swap or $93400000 or ;

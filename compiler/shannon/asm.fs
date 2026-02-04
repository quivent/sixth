\ asm.fs - x86-64 assembler abstraction (Shannon Layer 0)
\ Provides named words for x86-64 instruction emission.
\ Assumes: code-buf, code-pos, c,, d,, q, defined externally.

\ Comparison helpers (not in all Forth engines)
: >= ( a b -- flag ) < 0= ;
: <= ( a b -- flag ) > 0= ;

\ ============================================================
\ REGISTER CONSTANTS
\ ============================================================
\ x86-64 register encoding (3-bit values for ModR/M and REX)

0 constant RAX   1 constant RCX   2 constant RDX   3 constant RBX
4 constant RSP   5 constant RBP   6 constant RSI   7 constant RDI
8 constant R8    9 constant R9   10 constant R10  11 constant R11
12 constant R12  13 constant R13  14 constant R14  15 constant R15

\ ============================================================
\ REX PREFIX HELPERS
\ ============================================================
\ REX byte: 0100WRXB
\   W = 64-bit operand size
\   R = extends ModR/M reg field (for r8-r15)
\   X = extends SIB index
\   B = extends ModR/M r/m or SIB base (for r8-r15)

: rex.w ( -- )  $48 c, ;                      \ REX.W (64-bit operand)
: rex.wb ( reg -- )  8 >= if $49 else $48 then c, ;   \ REX.W + B if r8-r15
: rex.wr ( reg -- )  8 >= if $4c else $48 then c, ;   \ REX.W + R if r8-r15

: rex.wrb ( src dst -- )
  \ REX.W with R (src r8-r15) and B (dst r8-r15)
  swap 8 >= if 4 else 0 then    \ R bit
  swap 8 >= if 1 else 0 then    \ B bit
  or $48 or c, ;

\ ============================================================
\ MODR/M ENCODING
\ ============================================================
\ ModR/M byte: mod[2] reg[3] r/m[3]
\ mod=11 means register-direct

: modrm-rr ( src dst -- byte )
  \ Register-to-register: mod=11, reg=src[2:0], r/m=dst[2:0]
  7 and swap 7 and 3 lshift or $c0 or ;

: modrm-mr ( reg base -- byte )
  \ Memory indirect [base]: mod=00, reg=reg[2:0], r/m=base[2:0]
  7 and swap 7 and 3 lshift or ;

: modrm-mr8 ( reg base -- byte )
  \ Memory indirect [base+disp8]: mod=01, reg=reg[2:0], r/m=base[2:0]
  7 and swap 7 and 3 lshift or $40 or ;

: modrm-mr32 ( reg base -- byte )
  \ Memory indirect [base+disp32]: mod=10, reg=reg[2:0], r/m=base[2:0]
  7 and swap 7 and 3 lshift or $80 or ;

\ ============================================================
\ REGISTER-TO-REGISTER MOVES
\ ============================================================

: mov-rr ( src dst -- )
  \ MOV dst, src  (64-bit)
  2dup rex.wrb $89 c, modrm-rr c, ;

\ ============================================================
\ IMMEDIATE TO REGISTER
\ ============================================================

: mov-ri ( imm reg -- )
  \ MOV reg, imm64  (10 bytes: REX.W + B8+rd + imm64)
  dup rex.wb $b8 swap 7 and + c, q, ;

: mov-ri32 ( imm reg -- )
  \ MOV reg, imm32  (7 bytes: REX.W + C7 /0 + imm32, sign-extended)
  dup rex.wb $c7 c, 7 and $c0 or c, d, ;

: xor-rr-same ( reg -- )
  \ XOR reg, reg (zero register, 2-3 bytes, sets flags)
  dup 8 >= if dup rex.wrb else $48 c, then
  $31 c, dup modrm-rr c, ;

\ ============================================================
\ ARITHMETIC: REGISTER-TO-REGISTER
\ ============================================================

: add-rr ( src dst -- )  2dup rex.wrb $01 c, modrm-rr c, ;
: sub-rr ( src dst -- )  2dup rex.wrb $29 c, modrm-rr c, ;
: and-rr ( src dst -- )  2dup rex.wrb $21 c, modrm-rr c, ;
: or-rr  ( src dst -- )  2dup rex.wrb $09 c, modrm-rr c, ;
: xor-rr ( src dst -- )  2dup rex.wrb $31 c, modrm-rr c, ;
: cmp-rr ( src dst -- )  2dup rex.wrb $39 c, modrm-rr c, ;
: test-rr ( src dst -- ) 2dup rex.wrb $85 c, modrm-rr c, ;

: imul-rr ( src dst -- )
  \ IMUL dst, src (signed multiply)
  2dup rex.wrb $0f c, $af c, swap modrm-rr c, ;

: xchg-rr ( r1 r2 -- )
  \ XCHG r1, r2
  2dup rex.wrb $87 c, modrm-rr c, ;

\ ============================================================
\ ARITHMETIC: IMMEDIATE TO REGISTER (rax shortcuts)
\ ============================================================

: add-ri ( imm reg -- )
  \ ADD reg, imm32
  dup RAX = if drop rex.w $05 c, d, exit then
  rex.wb $81 c, 7 and $c0 or c, d, ;

: sub-ri ( imm reg -- )
  dup RAX = if drop rex.w $2d c, d, exit then
  rex.wb $81 c, 7 and $c0 or 5 3 lshift or c, d, ;

: and-ri ( imm reg -- )
  dup RAX = if drop rex.w $25 c, d, exit then
  rex.wb $81 c, 7 and $c0 or 4 3 lshift or c, d, ;

: or-ri ( imm reg -- )
  dup RAX = if drop rex.w $0d c, d, exit then
  rex.wb $81 c, 7 and $c0 or 1 3 lshift or c, d, ;

: xor-ri ( imm reg -- )
  dup RAX = if drop rex.w $35 c, d, exit then
  rex.wb $81 c, 7 and $c0 or 6 3 lshift or c, d, ;

: cmp-ri ( imm reg -- )
  dup RAX = if drop rex.w $3d c, d, exit then
  rex.wb $81 c, 7 and $c0 or 7 3 lshift or c, d, ;

: imul-ri ( imm reg -- )
  \ IMUL reg, reg, imm32
  dup rex.wb $69 c, dup modrm-rr c, swap d, ;

\ Small immediate variants (8-bit, saves space)
: add-ri8 ( imm reg -- )  rex.wb $83 c, 7 and $c0 or c, c, ;
: sub-ri8 ( imm reg -- )  rex.wb $83 c, 7 and $c0 or 5 3 lshift or c, c, ;

\ ============================================================
\ UNARY OPERATIONS
\ ============================================================

: inc-r ( reg -- )  dup rex.wb $ff c, 7 and $c0 or c, ;
: dec-r ( reg -- )  dup rex.wb $ff c, 7 and $c0 or 1 3 lshift or c, ;
: neg-r ( reg -- )  dup rex.wb $f7 c, 7 and $c0 or 3 3 lshift or c, ;
: not-r ( reg -- )  dup rex.wb $f7 c, 7 and $c0 or 2 3 lshift or c, ;

\ ============================================================
\ SHIFTS
\ ============================================================

: shl-ri ( imm reg -- )
  \ SHL reg, imm8
  dup rex.wb $c1 c, 7 and $c0 or 4 3 lshift or c, c, ;

: shr-ri ( imm reg -- )
  \ SHR reg, imm8 (logical)
  dup rex.wb $c1 c, 7 and $c0 or 5 3 lshift or c, c, ;

: sar-ri ( imm reg -- )
  \ SAR reg, imm8 (arithmetic)
  dup rex.wb $c1 c, 7 and $c0 or 7 3 lshift or c, c, ;

: shl-cl ( reg -- )
  \ SHL reg, cl
  dup rex.wb $d3 c, 7 and $c0 or 4 3 lshift or c, ;

: shr-cl ( reg -- )
  \ SHR reg, cl (logical)
  dup rex.wb $d3 c, 7 and $c0 or 5 3 lshift or c, ;

: sar-cl ( reg -- )
  \ SAR reg, cl (arithmetic)
  dup rex.wb $d3 c, 7 and $c0 or 7 3 lshift or c, ;

\ ============================================================
\ PUSH/POP
\ ============================================================

: push-r ( reg -- )
  dup 8 >= if $41 c, then $50 swap 7 and + c, ;

: pop-r ( reg -- )
  dup 8 >= if $41 c, then $58 swap 7 and + c, ;

: push-i8 ( imm -- )  $6a c, c, ;
: push-i32 ( imm -- ) $68 c, d, ;

\ ============================================================
\ MEMORY OPERATIONS
\ ============================================================

: mov-rm ( offset base dst -- )
  \ MOV dst, [base+offset]  (load)
  over 8 >= 2 pick 8 >= or if
    2 pick 8 >= if $4d else $4c then
    over 8 >= if 1 or then c,
  else rex.w then
  $8b c,
  over 0= if
    \ [base] - but rbp/r13 need disp8
    over 7 and 5 = if
      swap modrm-mr8 c, 0 c,
    else
      swap modrm-mr c,
    then
    drop
  else over dup -128 >= swap 127 <= and if
    \ [base+disp8]
    swap modrm-mr8 c, c,
  else
    \ [base+disp32]
    swap modrm-mr32 c, d,
  then then ;

: mov-mr ( src offset base -- )
  \ MOV [base+offset], src  (store)
  over 8 >= 2 pick 8 >= or if
    over 8 >= if $49 else $48 then
    2 pick 8 >= if 4 or then c,
  else rex.w then
  $89 c,
  over 0= if
    \ [base]
    over 7 and 5 = if
      modrm-mr8 c, drop 0 c,
    else
      modrm-mr c, drop
    then
  else over dup -128 >= swap 127 <= and if
    \ [base+disp8]
    modrm-mr8 c, c,
  else
    \ [base+disp32]
    modrm-mr32 c, d,
  then then ;

\ Byte variants
: movzx-rm8 ( offset base dst -- )
  \ MOVZX dst, byte [base+offset]
  rex.w $0f c, $b6 c,
  over 0= if
    over 7 and 5 = if swap modrm-mr8 c, drop 0 c, else swap modrm-mr c, drop then
  else over dup -128 >= swap 127 <= and if
    swap modrm-mr8 c, c,
  else
    swap modrm-mr32 c, d,
  then then ;

: mov-mr8 ( src offset base -- )
  \ MOV byte [base+offset], src (low 8 bits)
  $88 c,
  over 0= if
    over 7 and 5 = if modrm-mr8 c, drop 0 c, else modrm-mr c, drop then
  else over dup -128 >= swap 127 <= and if
    modrm-mr8 c, c,
  else
    modrm-mr32 c, d,
  then then ;

\ ============================================================
\ LEA
\ ============================================================

: lea ( offset base dst -- )
  \ LEA dst, [base+offset]
  over 8 >= 2 pick 8 >= or if
    2 pick 8 >= if $4d else $4c then
    over 8 >= if 1 or then c,
  else rex.w then
  $8d c,
  over 0= if
    over 7 and 5 = if swap modrm-mr8 c, drop 0 c, else swap modrm-mr c, drop then
  else over dup -128 >= swap 127 <= and if
    swap modrm-mr8 c, c,
  else
    swap modrm-mr32 c, d,
  then then ;

\ ============================================================
\ CONTROL FLOW
\ ============================================================

: call-rel ( offset -- )  $e8 c, d, ;
: jmp-rel ( offset -- )   $e9 c, d, ;

: jz-rel ( offset -- )    $0f c, $84 c, d, ;
: jnz-rel ( offset -- )   $0f c, $85 c, d, ;
: jl-rel ( offset -- )    $0f c, $8c c, d, ;
: jge-rel ( offset -- )   $0f c, $8d c, d, ;
: jle-rel ( offset -- )   $0f c, $8e c, d, ;
: jg-rel ( offset -- )    $0f c, $8f c, d, ;
: js-rel ( offset -- )    $0f c, $88 c, d, ;
: jns-rel ( offset -- )   $0f c, $89 c, d, ;
: jb-rel ( offset -- )    $0f c, $82 c, d, ;   \ unsigned <
: jae-rel ( offset -- )   $0f c, $83 c, d, ;   \ unsigned >=

\ Short jumps (8-bit displacement)
: jz-rel8 ( offset -- )   $74 c, c, ;
: jnz-rel8 ( offset -- )  $75 c, c, ;
: jmp-rel8 ( offset -- )  $eb c, c, ;

\ ============================================================
\ SPECIAL INSTRUCTIONS
\ ============================================================

: syscall ( -- )  $0f c, $05 c, ;
: ret ( -- )      $c3 c, ;
: nop ( -- )      $90 c, ;
: cqo ( -- )      rex.w $99 c, ;   \ sign-extend rax -> rdx:rax

\ Conditional moves
: cmovl-rr ( src dst -- )  2dup rex.wrb $0f c, $4c c, swap modrm-rr c, ;
: cmovg-rr ( src dst -- )  2dup rex.wrb $0f c, $4f c, swap modrm-rr c, ;
: cmovs-rr ( src dst -- )  2dup rex.wrb $0f c, $48 c, swap modrm-rr c, ;

\ SETcc (set byte based on condition)
: setz-r ( reg -- )   $0f c, $94 c, 7 and $c0 or c, ;
: setnz-r ( reg -- )  $0f c, $95 c, 7 and $c0 or c, ;
: setl-r ( reg -- )   $0f c, $9c c, 7 and $c0 or c, ;
: setg-r ( reg -- )   $0f c, $9f c, 7 and $c0 or c, ;
: setb-r ( reg -- )   $0f c, $92 c, 7 and $c0 or c, ;

\ Zero-extend byte to 64-bit
: movzx-r8 ( reg -- )
  \ MOVZX reg, reg[low8]
  rex.w $0f c, $b6 c, dup modrm-rr c, ;

\ ============================================================
\ MULTIPLY/DIVIDE
\ ============================================================

: mul-r ( reg -- )
  \ MUL rdx:rax = rax * reg (unsigned)
  dup rex.wb $f7 c, 7 and $c0 or 4 3 lshift or c, ;

: imul-r ( reg -- )
  \ IMUL rdx:rax = rax * reg (signed, one operand form)
  dup rex.wb $f7 c, 7 and $c0 or 5 3 lshift or c, ;

: div-r ( reg -- )
  \ DIV rax, rdx = rdx:rax / reg (unsigned)
  dup rex.wb $f7 c, 7 and $c0 or 6 3 lshift or c, ;

: idiv-r ( reg -- )
  \ IDIV rax, rdx = rdx:rax / reg (signed)
  dup rex.wb $f7 c, 7 and $c0 or 7 3 lshift or c, ;

\ ============================================================
\ STRING OPERATIONS
\ ============================================================

: rep-movsb ( -- )  $f3 c, $a4 c, ;  \ REP MOVSB: [rdi++] = [rsi++], rcx times
: rep-stosb ( -- )  $f3 c, $aa c, ;  \ REP STOSB: [rdi++] = al, rcx times

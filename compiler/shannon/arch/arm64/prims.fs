\ prims.fs - Arithmetic + bitwise primitives (Shannon Layer 1)
\ Depends on: asm.fs, stack.fs
\
\ Each word emits ARM64 instructions for one Forth primitive.
\ Convention: binary ops pop NOS into X9, operate with TOS (X19).

\ ============================================================
\ ARITHMETIC
\ ============================================================

: emit-add ( -- )     pop-nos  19  9 19 arm-add-reg emit32 ;   \ TOS = NOS + TOS
: emit-sub ( -- )     pop-nos  19  9 19 arm-sub-reg emit32 ;   \ TOS = NOS - TOS
: emit-mul ( -- )     pop-nos  19  9 19 arm-mul emit32 ;       \ TOS = NOS * TOS
: emit-div ( -- )     pop-nos  19  9 19 arm-sdiv emit32 ;      \ TOS = NOS / TOS
: emit-mod ( -- )                                               \ TOS = NOS mod TOS
  pop-nos
  10 9 19 arm-sdiv emit32            \ X10 = X9 / X19 (quotient)
  19 10 19 9 arm-msub emit32 ;       \ X19 = X9 - X10*X19 (remainder)

\ ============================================================
\ BITWISE
\ ============================================================

: emit-and ( -- )     pop-nos  19  9 19 arm-and-reg emit32 ;
: emit-or  ( -- )     pop-nos  19  9 19 arm-orr-reg emit32 ;
: emit-xor ( -- )     pop-nos  19  9 19 arm-eor-reg emit32 ;
: emit-invert ( -- )  19 19 arm-mvn emit32 ;                   \ TOS = NOT TOS
: emit-negate ( -- )  19 31 19 arm-sub-reg emit32 ;            \ TOS = 0 - TOS

\ ============================================================
\ SHIFTS
\ ============================================================

: emit-lshift ( -- )  pop-nos  19  9 19 arm-lslv emit32 ;      \ TOS = NOS << TOS
: emit-rshift ( -- )  pop-nos  19  9 19 arm-lsrv emit32 ;      \ TOS = NOS >> TOS (logical)

\ ============================================================
\ INCREMENT / DECREMENT
\ ============================================================

: emit-1+ ( -- )      19 19 1 arm-add-imm emit32 ;
: emit-1- ( -- )      19 19 1 arm-sub-imm emit32 ;

\ ABS - absolute value
: emit-abs ( -- )  \ ( n -- |n| )
  \ CMP X19, #0 ; CNEG X19, X19, LT
  19 0 arm-cmp-imm emit32            \ CMP X19, #0
  $DA93A673 emit32 ;                 \ CNEG X19, X19, LT (X19 = -X19 if negative)

\ ============================================================
\ COMPARISON
\ ============================================================

\ Helper: compare NOS to TOS, discard NOS, set flags
: cmp-pop ( -- )
  pop-nos                            \ X9 = NOS
  9 19 arm-cmp-reg emit32 ;          \ CMP X9, X19 (NOS vs TOS)

\ Helper: set TOS to -1 or 0 based on condition, then negate
\ Forth flags: -1 = true, 0 = false
\ CSET gives 1 for true, 0 for false; NEG converts 1 to -1
: emit-flag ( cond -- )
  cmp-pop
  19 swap arm-cset emit32            \ CSET X19, cond
  19 19 arm-neg emit32 ;             \ NEG X19, X19 (1 -> -1, 0 -> 0)

\ Condition codes for ARM64:
\ EQ=0, NE=1, HS/CS=2, LO/CC=3, MI=4, PL=5, VS=6, VC=7
\ HI=8, LS=9, GE=10, LT=11, GT=12, LE=13

: emit-= ( -- )  \ ( x y -- flag ) -1 if equal
  0 emit-flag ;                      \ EQ

: emit-<> ( -- )  \ ( x y -- flag ) -1 if not equal
  1 emit-flag ;                      \ NE

: emit-< ( -- )  \ ( x y -- flag ) -1 if NOS < TOS (signed)
  11 emit-flag ;                     \ LT

: emit-> ( -- )  \ ( x y -- flag ) -1 if NOS > TOS (signed)
  12 emit-flag ;                     \ GT

: emit-<= ( -- )  \ ( x y -- flag ) -1 if NOS <= TOS (signed)
  13 emit-flag ;                     \ LE

: emit->= ( -- )  \ ( x y -- flag ) -1 if NOS >= TOS (signed)
  10 emit-flag ;                     \ GE

: emit-u< ( -- )  \ ( x y -- flag ) -1 if NOS < TOS (unsigned)
  3 emit-flag ;                      \ LO/CC

: emit-u> ( -- )  \ ( x y -- flag ) -1 if NOS > TOS (unsigned)
  8 emit-flag ;                      \ HI

\ Zero comparisons (single operand)
: emit-0= ( -- )  \ ( x -- flag ) -1 if TOS is zero
  19 0 arm-cmp-imm emit32            \ CMP X19, #0
  19 0 arm-cset emit32               \ CSET X19, EQ
  19 19 arm-neg emit32 ;             \ NEG X19, X19

: emit-0<> ( -- )  \ ( x -- flag ) -1 if TOS is non-zero
  19 0 arm-cmp-imm emit32            \ CMP X19, #0
  19 1 arm-cset emit32               \ CSET X19, NE
  19 19 arm-neg emit32 ;             \ NEG X19, X19

: emit-0< ( -- )  \ ( x -- flag ) -1 if TOS is negative
  \ Arithmetic shift right by 63 gives -1 for negative, 0 otherwise
  19 19 63 arm-asr-imm emit32 ;      \ ASR X19, X19, #63

: emit-0> ( -- )  \ ( x -- flag ) -1 if TOS is positive (> 0)
  19 0 arm-cmp-imm emit32            \ CMP X19, #0
  19 12 arm-cset emit32              \ CSET X19, GT
  19 19 arm-neg emit32 ;             \ NEG X19, X19

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

: emit-0>= ( -- )  \ ( x -- flag ) -1 if TOS >= 0
  19 0 arm-cmp-imm emit32            \ CMP X19, #0
  19 10 arm-cset emit32              \ CSET X19, GE
  19 19 arm-neg emit32 ;             \ NEG X19, X19

: emit-0<= ( -- )  \ ( x -- flag ) -1 if TOS <= 0
  19 0 arm-cmp-imm emit32            \ CMP X19, #0
  19 13 arm-cset emit32              \ CSET X19, LE
  19 19 arm-neg emit32 ;             \ NEG X19, X19

\ ============================================================
\ CELL OPERATIONS
\ ============================================================

: emit-cells ( -- )  \ ( n -- n*8 )
  \ LSL X19, X19, #3 (shift left by 3 = multiply by 8)
  \ UBFM Xd, Xn, #(64-shift), #(63-shift) = LSL Xd, Xn, #shift
  \ LSL X19, X19, #3 = UBFM X19, X19, #61, #60
  $D37DF273 emit32 ;

: emit-cell+ ( -- )  \ ( addr -- addr+8 )
  19 19 8 arm-add-imm emit32 ;       \ ADD X19, X19, #8

\ ============================================================
\ MEMORY BLOCK OPERATIONS
\ ============================================================

: emit-move ( -- )  \ ( src dst u -- ) copy u bytes from src to dst
  \ Stack: TOS=u, NOS=dst, 3rd=src
  \ Handles overlapping by choosing direction:
  \   if dst > src: copy backward (from end)
  \   else: copy forward (from start)

  \ Save u to X12
  12 19 arm-mov-reg emit32            \ MOV X12, X19 (count)
  emit-drop                           \ TOS = dst

  \ Save dst to X11
  11 19 arm-mov-reg emit32            \ MOV X11, X19 (dst)
  emit-drop                           \ TOS = src

  \ X10 = src
  10 19 arm-mov-reg emit32            \ MOV X10, X19 (src)
  emit-drop                           \ pop src, stack now empty of args

  \ CBZ X12, done (skip if count=0)
  code-pos @                          \ save CBZ-to-done position
  12 0 arm-cbz emit32                 \ placeholder

  \ Compare dst vs src to decide direction
  \ CMP X11, X10 (dst - src)
  11 10 arm-cmp-reg emit32

  \ B.LS forward (if dst <= src, use forward copy)
  code-pos @                          \ save B.LS position
  0 arm-b emit32                      \ placeholder B (will patch to forward)
  \ Patch to B.LS (condition code 9 = LS = unsigned lower or same)
  code-buf code-pos @ + 4 - dup
  l@ $FF00000F and $54000009 or swap l!

  \ --- BACKWARD COPY (dst > src) ---
  \ Point to last bytes: src += count-1, dst += count-1
  10 10 12 arm-add-reg emit32         \ ADD X10, X10, X12 (src + count)
  10 10 1 arm-sub-imm emit32          \ SUB X10, X10, #1 (src + count - 1)
  11 11 12 arm-add-reg emit32         \ ADD X11, X11, X12 (dst + count)
  11 11 1 arm-sub-imm emit32          \ SUB X11, X11, #1 (dst + count - 1)

  \ back_loop:
  code-pos @                          \ save loop address

  9 10 -1 arm-ldrb-post emit32        \ LDRB W9, [X10], #-1 (load byte, src--)
  9 11 -1 arm-strb-post emit32        \ STRB W9, [X11], #-1 (store byte, dst--)
  12 12 1 arm-sub-imm emit32          \ SUB X12, X12, #1 (count--)

  \ CBNZ X12, back_loop
  dup code-pos @ - 4 /                \ offset = (loop - here) / 4
  12 swap arm-cbnz emit32
  drop                                \ drop back loop addr

  \ B done (skip forward copy)
  code-pos @                          \ save B-to-done position
  0 arm-b emit32                      \ placeholder

  \ Patch B.LS to jump here (forward copy)
  swap                                \ ( cbz-pos b-done-pos b-ls-pos )
  code-pos @ over - 4 /               \ offset to here
  $7FFFF and 5 lshift                 \ encode imm19
  swap code-buf + dup l@ $FF00001F and rot or swap l!

  \ --- FORWARD COPY ---
  \ fwd_loop:
  code-pos @                          \ save fwd loop address

  9 10 1 arm-ldrb-post emit32         \ LDRB W9, [X10], #1  (load byte, src++)
  9 11 1 arm-strb-post emit32         \ STRB W9, [X11], #1  (store byte, dst++)
  12 12 1 arm-sub-imm emit32          \ SUB X12, X12, #1 (count--)

  \ CBNZ X12, fwd_loop
  dup code-pos @ - 4 /                \ offset = (loop - here) / 4
  12 swap arm-cbnz emit32
  drop                                \ drop fwd loop addr

  \ done: patch both jumps (CBZ and B-to-done)
  \ Patch CBZ to here
  swap                                \ ( b-done-pos cbz-pos )
  code-pos @ over - 4 /               \ offset from CBZ to done
  12 swap arm-cbz swap patch32

  \ Patch B to here
  code-pos @ over - 4 /               \ offset from B to done
  swap code-buf + dup l@ $FC000000 and rot $3FFFFFF and or swap l! ;      \ patch CBZ

: emit-fill ( -- )  \ ( addr u c -- ) fill u bytes at addr with c
  \ Stack: TOS=c, NOS=u, 3rd=addr

  \ Save c (byte value) to X9
  9 19 arm-mov-reg emit32             \ MOV X9, X19 (char)
  emit-drop                           \ TOS = u

  \ Save u (count) to X12
  12 19 arm-mov-reg emit32            \ MOV X12, X19 (count)
  emit-drop                           \ TOS = addr

  \ X10 = addr (dest pointer)
  10 19 arm-mov-reg emit32            \ MOV X10, X19 (addr)
  emit-drop                           \ pop addr

  \ CBZ X12, done (skip if count=0)
  code-pos @                          \ save CBZ position
  12 0 arm-cbz emit32                 \ placeholder

  \ loop:
  code-pos @                          \ save loop address

  9 10 1 arm-strb-post emit32         \ STRB W9, [X10], #1 (store byte, addr++)
  12 12 1 arm-sub-imm emit32          \ SUB X12, X12, #1 (count--)

  \ CBNZ X12, loop
  over code-pos @ - 4 /               \ offset = (loop - here) / 4
  12 swap arm-cbnz emit32
  drop                                \ drop loop addr

  \ done: patch CBZ
  code-pos @ over - 4 /               \ offset from CBZ to done
  12 swap arm-cbz swap patch32 ;      \ patch CBZ

: emit-/string ( -- )  \ ( addr u n -- addr+n u-n ) adjust string
  \ TOS=n, NOS=u, 3rd=addr
  \ Result: TOS=u-n, NOS=addr+n

  \ X9 = n (TOS)
  9 19 arm-mov-reg emit32             \ MOV X9, X19
  emit-drop                           \ TOS = u

  \ TOS = u - n
  19 19 9 arm-sub-reg emit32          \ SUB X19, X19, X9 (u-n in TOS)

  \ Add n to addr (which is at [X22])
  10 22 0 arm-ldr-off emit32          \ LDR X10, [X22] (load addr)
  10 10 9 arm-add-reg emit32          \ ADD X10, X10, X9 (addr+n)
  10 22 0 arm-str-off emit32 ;        \ STR X10, [X22] (store back)

: emit-count ( -- )  \ ( c-addr -- addr u ) counted string to addr/len
  \ TOS = c-addr (pointer to counted string, first byte is length)
  \ Result: TOS = length, NOS = c-addr+1

  \ Load length byte into X9
  9 19 0 arm-ldrb-off emit32          \ LDRB W9, [X19] (length byte)

  \ Increment c-addr to get string start
  19 19 1 arm-add-imm emit32          \ ADD X19, X19, #1 (c-addr+1)

  \ Push the string address, then put length in TOS
  push-tos                            \ push c-addr+1
  19 9 arm-mov-reg emit32 ;           \ MOV X19, X9 (length in TOS)

\ prims.fs - Primitive code generators (Shannon Layer 2)
\ Pure codegen with no optimization logic.
\
\ Requires: asm.fs for x86-64 instructions
\ Requires: stack.fs for stack machine interface
\
\ Each word emits code for one Forth primitive.
\ The optimization layer (Layer 3) wraps these.

\ Dependencies: asm.fs and stack.fs must be loaded before this file
\ include compiler/shannon/asm.fs
\ include compiler/shannon/stack.fs

\ ============================================================
\ STACK MANIPULATION
\ ============================================================

: emit-dup ( -- )
  \ ( x -- x x ) duplicate TOS
  push-val ;

: emit-drop ( -- )
  \ ( x -- ) discard TOS
  pop-val ;

: emit-swap ( -- )
  \ ( x y -- y x ) exchange TOS and NOS
  tos nos xchg-rr ;

: emit-over ( -- )
  \ ( x y -- x y x ) copy NOS to TOS
  push-val
  nos tos mov-rr ;

: emit-rot ( -- )
  \ ( x y z -- y z x ) rotate 3rd to TOS
  tos third xchg-rr      \ z<->x: x y z -> z y x
  third nos xchg-rr ;    \ x<->y: z y x -> z x y? No...
  \ Actually: tos=z, nos=y, third=x
  \ After xchg tos,third: tos=x, nos=y, third=z
  \ After xchg third,nos: tos=x, nos=z, third=y
  \ Result: third=y, nos=z, tos=x = y z x. Correct!

: emit-nip ( -- )
  \ ( x y -- y ) discard NOS
  pop-nos-val ;

: emit-tuck ( -- )
  \ ( x y -- y x y ) copy TOS below NOS
  emit-swap emit-over ;

: emit-2dup ( -- )
  \ ( x y -- x y x y ) duplicate top pair
  push-val
  nos tos mov-rr         \ copy old NOS (now 3rd) to TOS
  push-val
  nos tos mov-rr ;       \ copy old TOS (now 3rd) to TOS

: emit-2drop ( -- )
  \ ( x y -- ) discard top pair
  pop-val pop-val ;

\ ============================================================
\ ARITHMETIC
\ ============================================================

: emit-add ( -- )
  \ ( x y -- x+y ) add NOS + TOS, result in TOS
  nos tos add-rr
  pop-nos-val ;

: emit-sub ( -- )
  \ ( x y -- x-y ) subtract NOS - TOS
  \ sub rbx, rax; mov rax, rbx
  tos nos sub-rr         \ nos = nos - tos
  nos tos mov-rr         \ tos = nos
  pop-nos-val ;

: emit-mul ( -- )
  \ ( x y -- x*y ) multiply NOS * TOS
  nos tos imul-rr
  pop-nos-val ;

: emit-negate ( -- )
  \ ( x -- -x ) negate TOS
  tos neg-r ;

: emit-1+ ( -- )
  \ ( x -- x+1 ) increment TOS
  tos inc-r ;

: emit-1- ( -- )
  \ ( x -- x-1 ) decrement TOS
  tos dec-r ;

\ ============================================================
\ FUSED IMMEDIATE OPERATIONS
\ ============================================================

: emit-add-imm ( n -- )
  \ ( x -- x+n ) add immediate to TOS
  tos add-ri ;

: emit-sub-imm ( n -- )
  \ ( x -- x-n ) subtract immediate from TOS
  tos sub-ri ;

: emit-mul-imm ( n -- )
  \ ( x -- x*n ) multiply TOS by immediate
  tos imul-ri ;

: emit-and-imm ( n -- )
  \ ( x -- x&n ) AND immediate with TOS
  tos and-ri ;

: emit-or-imm ( n -- )
  \ ( x -- x|n ) OR immediate with TOS
  tos or-ri ;

: emit-xor-imm ( n -- )
  \ ( x -- x^n ) XOR immediate with TOS
  tos xor-ri ;

: emit-lshift-imm ( n -- )
  \ ( x -- x<<n ) left shift TOS by n
  dup 1 = if
    drop tos tos add-rr  \ add rax,rax is shorter than shl rax,1
  else
    tos shl-ri
  then ;

\ ============================================================
\ COMPARISON
\ ============================================================

\ Helper: compare NOS to TOS, pop NOS
: cmp-nos-tos ( -- )
  \ cmp rbx, rax (compares nos TO tos, sets flags)
  tos nos cmp-rr
  pop-nos-val ;

\ Helper: set TOS to -1 or 0 based on SETcc byte
: emit-setcc ( cc -- )
  \ cc is the second byte of SETcc (e.g., $94 for SETZ)
  cmp-nos-tos
  $0f c, c, $c0 c,           \ SETcc al
  tos movzx-r8               \ movzx rax, al
  tos neg-r ;                \ neg rax (0->0, 1->-1)

: emit-= ( -- )
  \ ( x y -- flag ) -1 if equal, 0 otherwise
  $94 emit-setcc ;           \ SETZ

: emit-< ( -- )
  \ ( x y -- flag ) -1 if NOS < TOS (signed)
  $9c emit-setcc ;           \ SETL

: emit-> ( -- )
  \ ( x y -- flag ) -1 if NOS > TOS (signed)
  $9f emit-setcc ;           \ SETG

\ Helper: test TOS against itself, then SETcc
: emit-test-setcc ( cc -- )
  tos tos test-rr            \ test rax, rax
  $0f c, c, $c0 c,           \ SETcc al
  tos movzx-r8               \ movzx rax, al
  tos neg-r ;                \ neg rax

: emit-0= ( -- )
  \ ( x -- flag ) -1 if TOS is zero
  $94 emit-test-setcc ;      \ SETZ

: emit-0< ( -- )
  \ ( x -- flag ) -1 if TOS is negative
  \ Arithmetic right shift by 63 gives -1 for negative, 0 for non-negative
  63 tos sar-ri ;

: emit-0> ( -- )
  \ ( x -- flag ) -1 if TOS is positive (greater than zero)
  $9f emit-test-setcc ;      \ SETG (signed greater)

\ ============================================================
\ MEMORY
\ ============================================================

: emit-@ ( -- )
  \ ( addr -- x ) fetch 64-bit value from address in TOS
  0 tos tos mov-rm ;         \ mov rax, [rax]

: emit-! ( -- )
  \ ( x addr -- ) store x at addr
  \ addr=TOS=rax, x=NOS=rbx
  nos 0 tos mov-mr           \ mov [rax], rbx
  \ Now promote stack: drop both TOS and NOS
  pop-val                    \ rax = rbx (old NOS)
  pop-val ;                  \ rax = rcx (old 3rd)

: emit-c@ ( -- )
  \ ( addr -- c ) fetch byte from address in TOS
  0 tos tos movzx-rm8 ;      \ movzx rax, byte [rax]

: emit-c! ( -- )
  \ ( c addr -- ) store byte c at addr
  \ addr=TOS=rax, c=NOS=rbx
  nos 0 tos mov-mr8          \ mov byte [rax], bl
  pop-val pop-val ;

\ ============================================================
\ BITWISE
\ ============================================================

: emit-and ( -- )
  \ ( x y -- x&y ) bitwise AND
  nos tos and-rr
  pop-nos-val ;

: emit-or ( -- )
  \ ( x y -- x|y ) bitwise OR
  nos tos or-rr
  pop-nos-val ;

: emit-xor ( -- )
  \ ( x y -- x^y ) bitwise XOR
  nos tos xor-rr
  pop-nos-val ;

: emit-invert ( -- )
  \ ( x -- ~x ) bitwise NOT
  tos not-r ;

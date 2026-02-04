\ rstack.fs - Return Stack Operations (Shannon Layer 2c)
\ Ported from sixth.fs for the Shannon Architecture
\
\ Return stack pointer: RBP
\ Requires: asm.fs, stack.fs, c,, d,

\ ============================================================
\ RETURN STACK OPERATIONS
\ ============================================================

: gen->r ( -- )
  \ ( x -- ) R: ( -- x )
  \ sub rbp, 8; mov [rbp], rax; pop-val
  $48 c, $83 c, $ed c, 8 c,     \ sub rbp, 8
  $48 c, $89 c, $45 c, 0 c,     \ mov [rbp], rax
  pop-val ;

: gen-r> ( -- )
  \ ( -- x ) R: ( x -- )
  \ push-val; mov rax, [rbp]; add rbp, 8
  push-val
  $48 c, $8b c, $45 c, 0 c,     \ mov rax, [rbp]
  $48 c, $83 c, $c5 c, 8 c, ;   \ add rbp, 8

: gen-r@ ( -- )
  \ ( -- x ) R: ( x -- x )
  \ push-val; mov rax, [rbp]
  push-val
  $48 c, $8b c, $45 c, 0 c, ;   \ mov rax, [rbp]

: gen-2>r ( -- )
  \ ( x1 x2 -- ) R: ( -- x1 x2 )
  \ sub rbp, 16; mov [rbp+8], rbx; mov [rbp], rax; pop both
  $48 c, $83 c, $ed c, 16 c,    \ sub rbp, 16
  $48 c, $89 c, $5d c, 8 c,     \ mov [rbp+8], rbx
  pop-val
  $48 c, $89 c, $45 c, 0 c,     \ mov [rbp], rax
  pop-val ;

: gen-2r> ( -- )
  \ ( -- x1 x2 ) R: ( x1 x2 -- )
  push-val
  $48 c, $8b c, $45 c, 0 c,     \ mov rax, [rbp]
  push-val
  $48 c, $8b c, $45 c, 8 c,     \ mov rax, [rbp+8]
  $48 c, $83 c, $c5 c, 16 c, ;  \ add rbp, 16

: gen-2r@ ( -- )
  \ ( -- x1 x2 ) R: ( x1 x2 -- x1 x2 )
  push-val
  $48 c, $8b c, $45 c, 0 c,     \ mov rax, [rbp]
  push-val
  $48 c, $8b c, $45 c, 8 c, ;   \ mov rax, [rbp+8]


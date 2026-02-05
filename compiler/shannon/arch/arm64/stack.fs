\ stack.fs - Data stack operations + literal loading (Shannon Layer 1)
\ Depends on: asm.fs (code buffer + instruction encoders)
\
\ Register assignments:
\   X19 = TOS (top of stack, cached in register)
\   X22 = data stack pointer (grows downward)
\   X9  = scratch (NOS after pop, temp)
\   X10 = scratch (used by mod, etc.)
\   X28 = return stack pointer (future)
\
\ Stack convention:
\   push = STR X19, [X22, #-8]!   (pre-decrement store)
\   pop  = LDR Xn, [X22], #8      (post-increment load)

\ ============================================================
\ STACK PRIMITIVES
\ ============================================================

: push-tos ( -- )  \ Push TOS to memory stack
  19 22 -8 arm-str-pre emit32 ;

: pop-nos ( -- )   \ Pop memory stack into X9 (scratch)
  9 22 8 arm-ldr-post emit32 ;

: emit-drop ( -- )  \ Pop memory stack into TOS (discard old TOS)
  19 22 8 arm-ldr-post emit32 ;

\ ============================================================
\ LITERAL LOADING
\ ============================================================

\ Load 64-bit immediate into TOS (pushes old TOS first)
\ Uses MOVZ for bits[15:0], MOVK for each non-zero 16-bit chunk above.
: emit-lit ( n -- )
  push-tos
  dup $FFFF and 19 swap 0 arm-movz emit32
  dup 16 rshift $FFFF and ?dup if 19 swap 16 arm-movk emit32 then
  dup 32 rshift $FFFF and ?dup if 19 swap 32 arm-movk emit32 then
      48 rshift $FFFF and ?dup if 19 swap 48 arm-movk emit32 then ;

\ ============================================================
\ PROLOGUE / EPILOGUE
\ ============================================================

\ Entry: set up data stack pointer and return stack pointer below SP
: gen-prologue ( -- )
  22 31 2048 arm-sub-imm emit32     \ SUB X22, SP, #2048 (data stack)
  28 31 4096 arm-sub-imm emit32 ;   \ SUB X28, SP, #4096 (return stack)

\ Exit: TOS → exit code, terminate via SVC
: gen-epilogue ( -- )
  0 19 arm-mov-reg emit32            \ MOV X0, X19
  16 1 0 arm-movz emit32             \ MOVZ X16, #1
  $80 arm-svc emit32 ;               \ SVC #0x80

\ ============================================================
\ STACK MANIPULATION
\ ============================================================

: emit-dup ( -- )  \ ( x -- x x ) duplicate TOS
  push-tos ;

: emit-swap ( -- )  \ ( x y -- y x ) exchange TOS and NOS
  9 22 0 arm-ldr-off emit32          \ LDR X9, [X22] - load NOS into X9
  19 22 0 arm-str-off emit32         \ STR X19, [X22] - store TOS to NOS slot
  19 9 arm-mov-reg emit32 ;          \ MOV X19, X9 - X9 (old NOS) to TOS

: emit-over ( -- )  \ ( x y -- x y x ) copy NOS to TOS
  push-tos                           \ push old TOS
  19 22 1 arm-ldr-off emit32 ;       \ LDR X19, [X22, #8] - load NOS to TOS

: emit-rot ( -- )  \ ( x y z -- y z x ) rotate 3rd to TOS
  \ Stack: TOS=z, [X22]=y (NOS), [X22+8]=x (3rd)
  \ Want:  TOS=x, [X22]=z, [X22+8]=y
  9 22 0 arm-ldr-off emit32          \ X9 = y (NOS)
  10 22 1 arm-ldr-off emit32         \ X10 = x (3rd)
  19 22 0 arm-str-off emit32         \ [X22] = z (old TOS)
  9 22 1 arm-str-off emit32          \ [X22+8] = y
  19 10 arm-mov-reg emit32 ;         \ TOS = x

: emit-nip ( -- )  \ ( x y -- y ) discard NOS
  \ Just increment stack pointer, TOS unchanged
  22 22 8 arm-add-imm emit32 ;       \ ADD X22, X22, #8

: emit-tuck ( -- )  \ ( x y -- y x y ) copy TOS below NOS
  emit-swap emit-over ;

: emit-2dup ( -- )  \ ( x y -- x y x y ) duplicate top pair
  9 22 0 arm-ldr-off emit32          \ X9 = NOS (x)
  push-tos                           \ push y
  19 9 arm-mov-reg emit32            \ TOS = x
  push-tos                           \ push x
  9 22 1 arm-ldr-off emit32          \ X9 = y (now at [X22+8])
  19 9 arm-mov-reg emit32 ;          \ TOS = y

: emit-2drop ( -- )  \ ( x y -- ) discard top pair
  emit-drop emit-drop ;

: emit--rot ( -- )  \ ( x y z -- z x y ) reverse rotate
  \ Stack: TOS=z, [X22]=y, [X22+8]=x
  \ Want:  TOS=y, [X22]=x, [X22+8]=z
  9 22 0 arm-ldr-off emit32          \ X9 = y
  10 22 1 arm-ldr-off emit32         \ X10 = x
  19 22 1 arm-str-off emit32         \ [X22+8] = z (old TOS)
  10 22 0 arm-str-off emit32         \ [X22] = x
  19 9 arm-mov-reg emit32 ;          \ TOS = y

\ ============================================================
\ RETURN STACK OPERATIONS
\ ============================================================

: emit->r ( -- )  \ ( x -- ) ( R: -- x ) push TOS to return stack
  19 28 -8 arm-str-pre emit32        \ STR X19, [X28, #-8]!
  emit-drop ;                        \ pop data stack to TOS

: emit-r> ( -- )  \ ( -- x ) ( R: x -- ) pop return stack to TOS
  push-tos                           \ save current TOS
  19 28 8 arm-ldr-post emit32 ;      \ LDR X19, [X28], #8

: emit-r@ ( -- )  \ ( -- x ) ( R: x -- x ) copy top of return stack
  push-tos                           \ save current TOS
  19 28 0 arm-ldr-off emit32 ;       \ LDR X19, [X28] (no increment)

\ ============================================================
\ MEMORY OPERATIONS
\ ============================================================

: emit-@ ( -- )  \ ( addr -- value ) fetch 64-bit value
  19 19 0 arm-ldr-off emit32 ;       \ LDR X19, [X19]

: emit-! ( -- )  \ ( value addr -- ) store 64-bit value
  pop-nos                            \ X9 = value (was NOS)
  9 19 0 arm-str-off emit32          \ STR X9, [X19]
  emit-drop ;                        \ TOS = next item

: emit-c@ ( -- )  \ ( addr -- byte ) fetch byte (zero-extended)
  19 19 0 arm-ldrb-off emit32 ;      \ LDRB W19, [X19]

: emit-c! ( -- )  \ ( byte addr -- ) store byte
  pop-nos                            \ X9 = byte (was NOS)
  9 19 0 arm-strb-off emit32         \ STRB W9, [X19]
  emit-drop ;                        \ TOS = next item

: emit-+! ( -- )  \ ( n addr -- ) add n to memory cell
  pop-nos                            \ X9 = n (was NOS)
  10 19 0 arm-ldr-off emit32         \ LDR X10, [X19] (current value)
  10 10 9 arm-add-reg emit32         \ ADD X10, X10, X9
  10 19 0 arm-str-off emit32         \ STR X10, [X19]
  emit-drop ;                        \ TOS = next item

: emit-sp@ ( -- )  \ ( -- addr ) push data stack pointer
  push-tos                           \ save current TOS
  19 22 arm-mov-reg emit32 ;         \ MOV X19, X22

\ ============================================================
\ I/O OPERATIONS (macOS ARM64 syscalls)
\ ============================================================

: emit-emit ( -- )  \ ( c -- ) output character
  \ Store byte to stack, write(1, &byte, 1)
  19 31 -16 arm-str-pre emit32       \ STR X19, [SP, #-16]! (push to real stack, 16-aligned)
  0 1 0 arm-movz emit32              \ MOV X0, #1 (stdout)
  1 31 0 arm-add-imm emit32          \ ADD X1, SP, #0 (address of byte - can't use MOV for SP)
  2 1 0 arm-movz emit32              \ MOV X2, #1 (count)
  16 4 0 arm-movz emit32             \ MOV X16, #4 (write syscall)
  $80 arm-svc emit32                 \ SVC #0x80
  31 31 16 arm-add-imm emit32        \ ADD SP, SP, #16 (restore stack)
  emit-drop ;                        \ pop character from data stack

: emit-cr ( -- )  \ output newline
  push-tos                           \ save TOS
  19 10 0 arm-movz emit32            \ MOV X19, #10 (newline)
  emit-emit ;                        \ emit it

: emit-type ( -- )  \ ( addr len -- ) output string
  \ write(1, addr, len) - len is TOS, addr is NOS
  0 1 0 arm-movz emit32              \ MOV X0, #1 (stdout)
  2 19 arm-mov-reg emit32            \ MOV X2, X19 (len = TOS)
  emit-drop                          \ pop len, TOS = addr
  1 19 arm-mov-reg emit32            \ MOV X1, X19 (addr = TOS)
  emit-drop                          \ pop addr
  16 4 0 arm-movz emit32             \ MOV X16, #4 (write syscall)
  $80 arm-svc emit32 ;               \ SVC #0x80

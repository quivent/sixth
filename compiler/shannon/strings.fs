\ strings.fs - String Literals (s", .", [char])
\ Ported from sixth.fs for Shannon Architecture
\
\ Requires: stack.fs, io.fs, c,, d,, q,, code-here, input-buf, input-pos, input-len

\ ============================================================
\ S" - String Literal
\ ============================================================
\ s" text" - pushes (addr len) of string embedded in code

: gen-s" ( -- )
  \ Skip leading space after s"
  input-pos @ input-len @ < if
    input-buf input-pos @ + c@ 32 = if 1 input-pos +! then
  then
  \ Emit jmp over string data
  $e9 c, code-here >r 0 d,         \ jmp rel32, save patch addr
  code-here 0                       \ ( str-start len )
  begin
    input-pos @ input-len @ >= if true else
    input-buf input-pos @ + c@ [char] " = if
      1 input-pos +! true
    else
      input-buf input-pos @ + c@ c, 1+
      1 input-pos +! false
    then then
  until
  \ Patch jmp to skip string data
  code-here r> tuck -              \ ( len patch-addr displacement )
  swap 4 - code-buf + d!           \ write displacement
  \ Calculate runtime address: 0x4000b0 + code-offset
  swap $4000B0 + swap              \ ( runtime-addr len )
  \ Generate code to push addr and len
  push-val
  $48 c, $b8 c, swap q,            \ mov rax, addr
  push-val
  $48 c, $b8 c, q, ;               \ mov rax, len

\ ============================================================
\ ." - Print String Literal
\ ============================================================
\ ." text" - prints embedded string

: gen-dotquote ( -- )
  gen-s"
  gen-type ;

\ ============================================================
\ [CHAR] - Character Literal
\ ============================================================
\ [char] x - pushes ASCII value of next character

: compile-bracket-char ( -- )
  get-token dup 0= if 2drop exit then
  drop c@                          \ get first character
  ct-push ;                        \ push to compile-time stack

\ ============================================================
\ CHAR - Character Literal (immediate)
\ ============================================================

: compile-char ( -- )
  compile-bracket-char ;


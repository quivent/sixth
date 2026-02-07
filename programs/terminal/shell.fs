\ shell.fs - A minimal shell in Forth
\ Target: Shannon ARM64 macOS
\
\ Build: ./engine/fifth compiler/shannon-arm64.fs programs/terminal/shell.fs /tmp/shell
\ Run:   /tmp/shell
\
\ Available commands: pwd, echo, exit/quit
\ Example: echo "hello" | /tmp/shell

\ ============================================================
\ CONSTANTS
\ ============================================================
constant false 0
constant true -1
constant LINE-SIZE 256        \ max input line length
constant MAX-ARGS 16          \ max arguments per command

\ ============================================================
\ VARIABLES (static buffers)
\ ============================================================
\ Note: ARM64 compiler bug - allot doesn't work with constants, must use literals
create line-buf 256 allot          \ input line buffer (LINE-SIZE)
variable line-len                   \ current line length
create argv 256 allot              \ argument (addr,len) pairs - 16 * 2 * 8 = 256 bytes
variable argc                       \ argument count
variable exit-flag                  \ set to 1 to exit shell

\ ============================================================
\ LOW-LEVEL I/O
\ ============================================================

\ Read a line from stdin into line-buf
\ Returns: count of bytes read (0 = EOF)
: read-line ( -- n )
  line-buf LINE-SIZE 0 read-file drop   \ ( addr u fd -- u2 ior ) drop ior
  dup line-len ! ;

\ Print prompt
: prompt ( -- )
  s" $ " type ;

\ ============================================================
\ STRING UTILITIES
\ ============================================================

\ is-space: check if char is whitespace (space, tab, newline, CR)
: is-space ( c -- flag )
  dup 32 = over 9 = or over 10 = or swap 13 = or ;

\ Skip whitespace, return new address and remaining length
: skip-space ( addr len -- addr' len' )
  begin
    dup 0> if over c@ is-space else false then
  while
    swap 1+ swap 1-
  repeat ;

\ Find word length (count non-whitespace chars)
: word-len ( addr len -- n )
  0 -rot                  \ ( 0 addr len )
  begin
    dup 0> if over c@ is-space 0= else false then
  while
    rot 1+ -rot           \ increment count
    swap 1+ swap 1-       \ advance addr, decrease len
  repeat
  2drop ;

\ ============================================================
\ ARGUMENT ACCESS
\ ============================================================

\ Get nth argument as (addr len)
: arg@ ( n -- addr len )
  2 * cells argv +        \ point to entry
  dup @ swap cell+ @ ;    \ get addr and len

\ Store argument at nth position
: arg! ( addr len n -- )
  2 * cells argv +        \ point to entry
  rot over !              \ store addr
  cell+ ! ;               \ store len

\ ============================================================
\ COMMAND PARSING
\ ============================================================

\ Parse line-buf into argv array
: parse-line ( -- )
  0 argc !
  line-buf line-len @

  begin
    skip-space                   \ skip leading whitespace
    dup 0>
  while
    \ Calculate word length, save on R-stack
    2dup word-len >r             \ ( addr len ) R:( wlen )

    \ Store this arg: (addr, wlen)
    over r@                      \ ( addr len addr wlen ) R:( wlen )
    argc @ arg!                  \ ( addr len ) R:( wlen )
    argc @ 1+ argc !

    \ Advance past this word
    r>                           \ ( addr len wlen )
    rot over + -rot -            \ ( addr+wlen len-wlen )
  repeat
  2drop ;

\ ============================================================
\ BUILT-IN COMMANDS
\ ============================================================

\ Exit the shell (sets flag, main loop will terminate)
: cmd-exit ( -- )
  1 exit-flag ! ;

\ Print working directory (placeholder)
: cmd-pwd ( -- )
  s" /Users/joshkornreich/sixth" type cr ;

\ Echo arguments
: cmd-echo ( -- )
  argc @ 1 > if
    argc @ 1 do
      i arg@ type
      i 1+ argc @ < if 32 emit then
    loop
  then
  cr ;

\ ============================================================
\ COMMAND DISPATCH
\ ============================================================

\ Compare two strings (simple byte-by-byte)
: str= ( addr1 len1 addr2 len2 -- flag )
  rot over <> if 2drop drop false exit then  \ lengths differ
  \ Now: addr1 len2 addr2, and len1=len2
  swap >r                \ addr2 len r: addr1
  begin
    dup 0>
  while
    over c@              \ addr2 len c2
    r@ c@                \ addr2 len c2 c1
    <> if
      2drop r> drop false exit
    then
    1- swap 1+ swap      \ len-1 addr2+1
    r> 1+ >r             \ addr1+1
  repeat
  2drop r> drop true ;

\ Execute parsed command
: exec-cmd ( -- )
  argc @ 0= if exit then     \ empty line

  \ Get first argument (command name)
  0 arg@

  \ Check builtins
  2dup s" exit" str= if 2drop cmd-exit exit then
  2dup s" quit" str= if 2drop cmd-exit exit then
  2dup s" pwd"  str= if 2drop cmd-pwd  exit then
  2dup s" echo" str= if 2drop cmd-echo exit then

  \ Unknown command
  type s" : command not found" type cr ;

\ ============================================================
\ MAIN LOOP
\ ============================================================

\ Strip trailing newline from line-buf
: strip-newline ( -- )
  line-len @ 0> if
    line-buf line-len @ 1- + c@ 10 = if
      line-len @ 1- line-len !
    then
  then ;

: shell ( -- )
  0 exit-flag !
  begin
    exit-flag @ 0=           \ not exiting?
  while
    prompt
    read-line 0= if
      1 exit-flag !          \ EOF - exit
    else
      strip-newline
      parse-line
      exec-cmd
    then
  repeat
  cr s" Goodbye!" type cr ;

\ Entry point
: main shell ;

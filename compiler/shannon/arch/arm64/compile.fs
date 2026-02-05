\ compile.fs - Minimal compiler: tokenizer + dispatch (Shannon Layer 3)
\ Depends on: asm.fs, stack.fs, prims.fs, macho.fs
\
\ This is a minimal compiler that can handle:
\   : main <body> ;
\ Where body contains: literals, +, -, *, /, mod, and, or, xor, etc.
\
\ No optimization. No forward references. No variables. Just compilation.

\ ============================================================
\ INPUT BUFFER
\ ============================================================

153600 constant INPUT-SIZE
create input-buf INPUT-SIZE allot
variable input-pos   0 input-pos !
variable input-len   0 input-len !

\ ============================================================
\ STRING COMPARISON
\ ============================================================

: str= ( addr1 u1 addr2 u2 -- flag )
  rot over <> if 2drop drop false exit then
  dup 0= if 2drop drop true exit then
  0 ?do
    over i + c@ over i + c@ <> if 2drop false unloop exit then
  loop
  2drop true ;

\ ============================================================
\ TOKENIZER
\ ============================================================

: skip-ws ( -- )
  begin
    input-pos @ input-len @ < 0= if exit then
    input-buf input-pos @ + c@ 33 < if
      1 input-pos +!
    else
      exit
    then
  again ;

: skip-line ( -- )
  begin
    input-pos @ input-len @ < 0= if exit then
    input-buf input-pos @ + c@ 10 = if 1 input-pos +! exit then
    1 input-pos +!
  again ;

: get-token ( -- addr u | 0 0 )
  skip-ws
  input-pos @ input-len @ < 0= if 0 0 exit then
  \ Skip line comments (\)
  input-buf input-pos @ + c@ [char] \ = if
    skip-line
    recurse exit
  then
  \ Skip paren comments ( ... )
  input-buf input-pos @ + c@ [char] ( = if
    input-pos @ 1+ input-len @ < if
      input-buf input-pos @ 1+ + c@ 33 < if
        1 input-pos +!
        begin
          input-pos @ input-len @ < 0= if 0 0 exit then
          input-buf input-pos @ + c@ [char] ) = if
            1 input-pos +! recurse exit
          then
          1 input-pos +!
        again
      then
    then
  then
  \ Return token
  input-buf input-pos @ +
  0 begin
    input-pos @ input-len @ < while
    input-buf input-pos @ + c@ 32 > while
    1 input-pos +!
    1+
  repeat then ;

\ ============================================================
\ NUMBER PARSING
\ ============================================================

: digit? ( c -- flag )
  dup [char] 0 < if drop false exit then
  [char] 9 > if false else true then ;

: hex-digit? ( c -- flag )
  dup digit? if drop true exit then
  dup [char] a < if
    dup [char] A < if drop false exit then
    [char] F > if false else true then
  else
    [char] f > if false else true then
  then ;

: char>digit ( c -- n )
  dup digit? if [char] 0 - exit then
  dup [char] a < 0= if [char] a 10 - - exit then
  [char] A 10 - - ;

variable parse-addr   \ temp storage for parsing

: parse-unsigned ( addr u -- n true | false )
  dup 0= if 2drop false exit then
  over c@ digit? 0= if 2drop false exit then
  swap parse-addr !   \ ( u )
  0 swap 0 ?do        \ ( acc )
    parse-addr @ i + c@ digit? 0= if drop false unloop exit then
    10 * parse-addr @ i + c@ [char] 0 - +
  loop
  true ;

: parse-number ( addr u -- n true | false )
  dup 0= if 2drop false exit then
  \ Check for leading minus sign
  over c@ [char] - = if
    1- swap 1+ swap                \ skip minus: ( addr+1 u-1 )
    parse-unsigned if negate true else false then
    exit
  then
  parse-unsigned ;

\ ============================================================
\ SIMPLE DICTIONARY (for multi-word support)
\ ============================================================

\ Each entry: 16 bytes name (padded), 8 bytes code-addr
24 constant DICT-ENTRY-SIZE
create dict-buf 256 DICT-ENTRY-SIZE * allot
variable dict-count  0 dict-count !
variable entry-var   \ temp storage for dict-name= (can't use >r in nested loops)

: dict-entry ( n -- addr ) DICT-ENTRY-SIZE * dict-buf + ;

: dict-add ( addr u code-addr -- )
  \ Add word to dictionary
  dict-count @ dict-entry      \ get entry address
  dup 16 0 fill                \ clear name area
  >r                           \ save entry addr
  r@ 16 + !                    \ store code-addr at offset 16
  r@ swap 16 min move          \ copy name (max 16 chars)
  r> drop
  1 dict-count +! ;

: dict-name= ( addr1 u1 entry -- flag )
  \ Compare name with dictionary entry name (u1 chars only)
  \ NOTE: Use variable, not >r, because this is called from within ?do loop
  entry-var !                 \ save entry addr in variable
  dup 16 > if drop 16 then    \ limit lookup len to 16
  dup 0 ?do
    over i + c@ entry-var @ i + c@ <> if 2drop false unloop exit then
  loop
  2drop true ;

: dict-find ( addr u -- code-addr true | false )
  \ Look up word in dictionary
  \ Returns code-addr and true if found, just false if not found
  dict-count @ 0 ?do
    2dup i dict-entry dict-name= if
      2drop i dict-entry 16 + @ true unloop exit
    then
  loop
  2drop false ;

\ ============================================================
\ DISPATCH TABLE
\ ============================================================

: try-arith ( addr u -- handled? )
  2dup s" +" str= if 2drop emit-add true exit then
  2dup s" -" str= if 2drop emit-sub true exit then
  2dup s" *" str= if 2drop emit-mul true exit then
  2dup s" /" str= if 2drop emit-div true exit then
  2dup s" mod" str= if 2drop emit-mod true exit then
  2dup s" and" str= if 2drop emit-and true exit then
  2dup s" or" str= if 2drop emit-or true exit then
  2dup s" xor" str= if 2drop emit-xor true exit then
  2dup s" invert" str= if 2drop emit-invert true exit then
  2dup s" negate" str= if 2drop emit-negate true exit then
  2dup s" abs" str= if 2drop emit-abs true exit then
  2dup s" lshift" str= if 2drop emit-lshift true exit then
  2dup s" rshift" str= if 2drop emit-rshift true exit then
  2dup s" 1+" str= if 2drop emit-1+ true exit then
  2dup s" 1-" str= if 2drop emit-1- true exit then
  2drop false ;

: try-stack ( addr u -- handled? )
  2dup s" drop" str= if 2drop emit-drop true exit then
  2dup s" dup" str= if 2drop emit-dup true exit then
  2dup s" swap" str= if 2drop emit-swap true exit then
  2dup s" over" str= if 2drop emit-over true exit then
  2dup s" rot" str= if 2drop emit-rot true exit then
  2dup s" nip" str= if 2drop emit-nip true exit then
  2dup s" tuck" str= if 2drop emit-tuck true exit then
  2dup s" 2dup" str= if 2drop emit-2dup true exit then
  2dup s" 2drop" str= if 2drop emit-2drop true exit then
  2dup s" -rot" str= if 2drop emit--rot true exit then
  2dup s" >r" str= if 2drop emit->r true exit then
  2dup s" r>" str= if 2drop emit-r> true exit then
  2dup s" r@" str= if 2drop emit-r@ true exit then
  2dup s" emit" str= if 2drop emit-emit true exit then
  2dup s" cr" str= if 2drop emit-cr true exit then
  2dup s" type" str= if 2drop emit-type true exit then
  2dup s" @" str= if 2drop emit-@ true exit then
  2dup s" !" str= if 2drop emit-! true exit then
  2dup s" c@" str= if 2drop emit-c@ true exit then
  2dup s" c!" str= if 2drop emit-c! true exit then
  2dup s" +!" str= if 2drop emit-+! true exit then
  2dup s" sp@" str= if 2drop emit-sp@ true exit then
  2drop false ;

: try-compare ( addr u -- handled? )
  2dup s" =" str= if 2drop emit-= true exit then
  2dup s" <>" str= if 2drop emit-<> true exit then
  2dup s" <" str= if 2drop emit-< true exit then
  2dup s" >" str= if 2drop emit-> true exit then
  2dup s" <=" str= if 2drop emit-<= true exit then
  2dup s" >=" str= if 2drop emit->= true exit then
  2dup s" u<" str= if 2drop emit-u< true exit then
  2dup s" u>" str= if 2drop emit-u> true exit then
  2dup s" 0=" str= if 2drop emit-0= true exit then
  2dup s" 0<>" str= if 2drop emit-0<> true exit then
  2dup s" 0<" str= if 2drop emit-0< true exit then
  2dup s" 0>" str= if 2drop emit-0> true exit then
  2drop false ;

\ String literals
: compile-dot-quote ( -- )
  \ Read and compile string until closing quote
  \ Skip initial space after ."
  input-pos @ input-len @ < if
    input-buf input-pos @ + c@ 32 = if 1 input-pos +! then
  then
  \ Emit each character until "
  begin
    input-pos @ input-len @ < while
    input-buf input-pos @ + c@
    dup [char] " = if drop 1 input-pos +! exit then
    emit-lit emit-emit
    1 input-pos +!
  repeat ;

: try-string ( addr u -- handled? )
  2dup s\" .\"" str= if 2drop compile-dot-quote true exit then
  2drop false ;

\ Control flow handling - requires cf-push/cf-pop from control.fs
: try-control ( addr u -- handled? )
  2dup s" if" str= if 2drop gen-if cf-push true exit then
  2dup s" then" str= if 2drop cf-pop gen-then true exit then
  2dup s" else" str= if 2drop cf-pop gen-else cf-push true exit then
  2dup s" begin" str= if 2drop gen-begin cf-push true exit then
  2dup s" until" str= if 2drop cf-pop gen-until true exit then
  2dup s" again" str= if 2drop cf-pop gen-again true exit then
  2dup s" while" str= if 2drop cf-pop gen-while cf-push cf-push true exit then
  2dup s" repeat" str= if 2drop cf-pop cf-pop gen-repeat true exit then
  \ Do/loop control flow
  2dup s" do" str= if 2drop gen-do cf-push cf-push true exit then
  2dup s" loop" str= if 2drop cf-pop cf-pop gen-loop true exit then
  2dup s" +loop" str= if 2drop cf-pop cf-pop gen-+loop true exit then
  2dup s" i" str= if 2drop emit-i true exit then
  2dup s" j" str= if 2drop emit-j true exit then
  2dup s" leave" str= if 2drop gen-leave true exit then
  2dup s" unloop" str= if 2drop emit-unloop true exit then
  2drop false ;

: compile-token ( addr u -- )
  2dup parse-number if
    \ Stack: ( addr u n ) - need to drop addr u, keep n
    nip nip emit-lit exit
  then
  2dup try-arith if 2drop exit then
  2dup try-stack if 2drop exit then
  2dup try-compare if 2drop exit then
  2dup try-string if 2drop exit then
  2dup try-control if 2drop exit then
  \ Check dictionary for word call
  2dup dict-find if
    nip nip gen-call exit
  then
  \ Unknown token - error
  ." Unknown word: " type cr
  1 throw ;

\ ============================================================
\ COLON DEFINITION COMPILER
\ ============================================================

: compile-colon ( -- )
  get-token                    \ get word name
  2dup s" main" str= if
    \ Main word - record entry point for Mach-O, emit prologue
    2drop code-here main-entry ! gen-prologue
    begin
      get-token
      dup 0= if 2drop ." Unexpected end of input" cr 1 throw then
      2dup s" ;" str= if 2drop gen-epilogue exit then
      compile-token
    again
  else
    \ Regular word - record address, save LR for nested calls
    code-here dict-add         \ add to dictionary
    gen-word-prologue          \ save LR to return stack
    begin
      get-token
      dup 0= if 2drop ." Unexpected end of input" cr 1 throw then
      2dup s" ;" str= if 2drop gen-ret exit then
      compile-token
    again
  then ;

\ ============================================================
\ TOP-LEVEL COMPILER
\ ============================================================

: compile-source ( addr u -- )
  \ Copy source to input buffer
  dup input-len !
  input-buf swap move
  0 input-pos !
  0 code-pos !
  \ Parse and compile
  begin
    get-token
    dup 0= if 2drop exit then
    2dup s" :" str= if
      2drop compile-colon
    else
      2drop   \ skip unknown top-level tokens for now
    then
  again ;

: compile-string ( addr u -- )
  compile-source
  build-macho ;

\ ============================================================
\ COMMAND-LINE ENTRY POINT
\ ============================================================

: usage ( -- )
  ." Usage: fifth compiler/shannon-arm64.fs <source.fs> <output>" cr
  1 throw ;

: run ( -- )
  argc 4 < if usage then
  2 argv slurp-file compile-string
  3 argv save-binary
  bye ;
run

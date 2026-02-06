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
\ FORWARD REFERENCES
\ ============================================================
\ Each pending ref: 16 bytes name (padded), 8 bytes call-site (code offset)
24 constant PEND-ENTRY-SIZE
create pend-buf 256 PEND-ENTRY-SIZE * allot
variable pend-count  0 pend-count !

: pend-entry ( n -- addr ) PEND-ENTRY-SIZE * pend-buf + ;

: add-pending ( addr u call-site -- )
  \ Record a forward reference: name at addr/u, BL at call-site
  pend-count @ pend-entry    \ get entry address
  dup 16 0 fill              \ clear name area
  >r
  r@ 16 + !                  \ store call-site at offset 16
  r@ swap 16 min move        \ copy name (max 16 chars)
  r> drop
  1 pend-count +! ;

: resolve-pending ( name-addr name-len target -- )
  \ Patch all pending calls to this word
  \ Scan backwards so we can remove resolved entries
  pend-count @ 0 ?do
    pend-count @ 1- i - pend-entry  \ iterate from end
    >r
    2dup r@ swap 16 min          \ ( name-addr name-len target entry name-addr len )
    2swap drop                   \ ( name-addr name-len target name-addr len entry )
    -rot                         \ ( name-addr name-len target entry name-addr len )
    r@ dict-name= if             \ compare names
      \ Match found - patch and mark for removal
      dup r@ 16 + @              \ ( name-addr name-len target target call-site )
      patch-call                 \ patch the BL instruction
      \ Mark entry as resolved by zeroing name
      r@ 16 0 fill
    then
    r> drop
  loop
  2drop drop ;

: check-unresolved ( -- )
  \ Error if any unresolved forward references remain
  pend-count @ 0 ?do
    i pend-entry c@ 0 <> if      \ non-zero first char = unresolved
      ." Unresolved forward reference: "
      i pend-entry 16 type cr
      1 throw
    then
  loop ;

\ ============================================================
\ CONSTANTS
\ ============================================================
\ Each entry: 16 bytes name (padded), 8 bytes value
24 constant CONST-ENTRY-SIZE
create const-buf 128 CONST-ENTRY-SIZE * allot
variable const-count  0 const-count !

: const-entry ( n -- addr ) CONST-ENTRY-SIZE * const-buf + ;

: const-add ( value addr u -- )
  \ Add constant to table: value is TOS, name is addr/u
  const-count @ const-entry    \ get entry address
  dup 16 0 fill                \ clear name area
  >r
  r@ swap 16 min move          \ copy name (max 16 chars)
  r@ 16 + !                    \ store value at offset 16
  r> drop
  1 const-count +! ;

: const-find ( addr u -- value true | false )
  \ Look up constant, return value if found
  const-count @ 0 ?do
    2dup i const-entry dict-name= if
      2drop i const-entry 16 + @ true unloop exit
    then
  loop
  2drop false ;

\ ============================================================
\ VARIABLES (stack-based: X20 + offset)
\ ============================================================
\ Each entry: 16 bytes name (padded), 8 bytes offset from X20 base
24 constant VAR-ENTRY-SIZE
create var-buf 128 VAR-ENTRY-SIZE * allot
variable var-count  0 var-count !
variable var-next   8 var-next !    \ next offset (8 = skip here pointer at offset 0)

: var-entry ( n -- addr ) VAR-ENTRY-SIZE * var-buf + ;

: var-add ( addr u -- )
  \ Add variable to table, allocate 8 bytes in DATA segment
  var-count @ var-entry    \ get entry address
  dup 16 0 fill            \ clear name area
  >r
  r@ swap 16 min move      \ copy name (max 16 chars)
  var-next @ r@ 16 + !     \ store current offset at offset 16
  r> drop
  8 var-next +!            \ advance next offset by 8 (cell size)
  1 var-count +! ;

: var-find ( addr u -- offset true | false )
  \ Look up variable, return offset from X20 base if found
  var-count @ 0 ?do
    2dup i var-entry dict-name= if
      2drop i var-entry 16 + @ true unloop exit
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
  2dup s" cells" str= if 2drop emit-cells true exit then
  2dup s" cell+" str= if 2drop emit-cell+ true exit then
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
  2dup s" here" str= if 2drop emit-here true exit then
  2dup s" allot" str= if 2drop emit-allot true exit then
  2dup s" ," str= if 2drop emit-comma true exit then
  2dup s" c," str= if 2drop emit-c-comma true exit then
  2dup s" open-file" str= if 2drop emit-open-file true exit then
  2dup s" close-file" str= if 2drop emit-close-file true exit then
  2dup s" write-file" str= if 2drop emit-write-file true exit then
  2dup s" read-file" str= if 2drop emit-read-file true exit then
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
  2dup s" 0>=" str= if 2drop emit-0>= true exit then
  2dup s" 0<=" str= if 2drop emit-0<= true exit then
  2drop false ;

\ String literals
256 constant STR-BUF-SIZE
create str-temp STR-BUF-SIZE allot    \ temp buffer for parsing strings
variable str-len                       \ length of parsed string

: parse-string-to-temp ( -- )
  \ Parse string from input until closing ", store in str-temp
  \ Skip initial space after s" or ."
  0 str-len !
  input-pos @ input-len @ < if
    input-buf input-pos @ + c@ 32 = if 1 input-pos +! then
  then
  \ Copy characters until "
  begin
    input-pos @ input-len @ < while
    input-buf input-pos @ + c@
    dup [char] " = if drop 1 input-pos +! exit then
    str-temp str-len @ + c!
    1 str-len +!
    1 input-pos +!
  repeat ;

: compile-s-quote ( -- )
  \ Parse string, emit inline with branch, push addr/len at runtime
  parse-string-to-temp
  str-len @ 0= if
    \ Empty string: push some address and 0 length
    0 emit-lit 0 emit-lit exit
  then

  \ Calculate padded length (round up to 4 bytes)
  str-len @ 3 + 3 invert and          \ ( padded-len )

  \ Calculate branch offset: skip B instruction (1) + padded bytes / 4
  dup 2 rshift 1+                     \ ( padded-len branch-offset )

  \ Emit B instruction to skip over string
  arm-b emit32                        \ ( padded-len )

  \ Remember where string starts (for address calculation)
  code-pos @                          \ ( padded-len string-code-pos )

  \ Emit string bytes
  str-len @ 0 ?do
    str-temp i + c@ >code
  loop

  \ Pad to 4-byte boundary
  str-len @ 3 and ?dup if
    4 swap - 0 ?do 0 >code loop
  then
  nip                                 \ ( string-code-pos ) - drop padded-len, keep string-code-pos

  \ Now emit code to push address and length using PC-relative ADR
  \ ADR calculates: PC + offset. We need offset = string_pos - adr_pos
  push-tos                            \ save TOS to make room for address
  dup code-pos @ -                    \ ( string-code-pos offset ) where offset = string_pos - adr_pos (negative)
  19 swap arm-adr emit32              \ ADR X19, #offset (loads string addr into TOS)
  drop                                \ drop string-code-pos
  str-len @ emit-lit ;                \ push length as literal

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
  2dup s\" s\"" str= if 2drop compile-s-quote true exit then
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
  2dup s" exit" str= if 2drop emit-exit true exit then
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
  \ Check constants - emit literal value
  2dup const-find if
    nip nip emit-lit exit
  then
  \ Check variables - emit stack-relative address (X20 + offset)
  2dup var-find if
    nip nip emit-var-addr exit
  then
  \ Check dictionary for word call
  2dup dict-find if
    nip nip gen-call exit
  then
  \ Forward reference - emit placeholder BL, record for patching
  code-here add-pending          \ record (name, call-site)
  0 arm-bl emit32 ;              \ emit BL #0 placeholder

\ ============================================================
\ COLON DEFINITION COMPILER
\ ============================================================

: compile-colon ( -- )
  get-token                    \ get word name
  2dup s" main" str= if
    \ Main word - record entry point for Mach-O, emit prologue
    2drop code-here main-entry ! var-next @ gen-prologue
    begin
      get-token
      dup 0= if 2drop ." Unexpected end of input" cr 1 throw then
      2dup s" ;" str= if 2drop gen-epilogue exit then
      compile-token
    again
  else
    \ Regular word - record address, save LR for nested calls
    2dup code-here dict-add    \ add to dictionary (keep name for resolve)
    code-here resolve-pending  \ resolve any forward refs to this word
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

: compile-constant ( -- )
  \ Syntax: <value> constant <name>
  \ At top-level, we parse: constant <name> <value>
  \ This differs from standard Forth but is easier to parse
  get-token                      \ get name
  dup 0= if 2drop ." Missing constant name" cr 1 throw then
  get-token                      \ get value
  dup 0= if 2drop 2drop ." Missing constant value" cr 1 throw then
  parse-number 0= if
    ." Constant value must be a number" cr 1 throw
  then
  -rot const-add ;               \ ( value name-addr name-u -- )

: compile-variable ( -- )
  \ Syntax: variable <name>
  get-token                      \ get name
  dup 0= if 2drop ." Missing variable name" cr 1 throw then
  var-add ;                      \ add to var-buf, allocate 8 bytes in DATA segment

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
      2dup s" constant" str= if
        2drop compile-constant
      else
        2dup s" variable" str= if
          2drop compile-variable
        else
          2drop   \ skip unknown top-level tokens for now
        then
      then
    then
  again ;

: compile-string ( addr u -- )
  compile-source
  check-unresolved
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

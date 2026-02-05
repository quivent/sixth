\ main.fs - Shannon Compiler Entry Point
\ Wires together all Shannon modules into a working compiler.
\
\ This is GLUE code. It provides infrastructure other modules need
\ and orchestrates the two-pass compilation pipeline:
\   Pass 1 (scan-all): Gather word metadata, build info-buf
\   Pass 2 (compile-all): Generate native x86-64 code
\
\ Usage: ./engine/fifth compiler/shannon/main.fs input.fs output
\
\ CURRENT STATUS: Compiles subset of Forth with optimization.
\ Implemented: + - * and or xor negate 1+ 1- invert 2* 2/
\              dup drop swap over rot nip tuck 2dup 2drop
\              = < > 0= 0< 0> @ ! c@ c! numbers user-words
\ Not yet: . cr emit type if/then/else begin/while/repeat
\          do/loop >r r> and other control flow / I/O

\ ============================================================
\ BUFFER SIZES
\ ============================================================

262144 constant CODE-SIZE     \ 256KB code buffer
150000 constant INPUT-SIZE    \ 150KB input buffer
1024 constant DICT-SIZE       \ Max user-defined words

\ ============================================================
\ CODE EMISSION INFRASTRUCTURE
\ ============================================================
\ Required by: asm.fs, elf.fs, all codegen

create code-buf CODE-SIZE allot
variable code-pos  0 code-pos !

: c, ( b -- ) code-buf code-pos @ + c!  1 code-pos +! ;
: d, ( d -- ) dup c, 8 rshift dup c, 8 rshift dup c, 8 rshift c, ;
: q, ( q -- ) dup d, 32 rshift d, ;
: code-here ( -- addr ) code-pos @ ;

\ Store 32-bit value at address (for patching)
: d! ( n addr -- )
  over $ff and over c!
  swap 8 rshift swap 1+
  over $ff and over c!
  swap 8 rshift swap 1+
  over $ff and over c!
  swap 8 rshift swap 1+ c! ;

\ ============================================================
\ COMPARISON HELPERS (not in all Forth engines)
\ ============================================================

: >= ( a b -- flag ) < 0= ;
: <= ( a b -- flag ) > 0= ;

\ Standard Forth constants (needed for self-hosting)
0 constant false
-1 constant true

\ ============================================================
\ INPUT BUFFER INFRASTRUCTURE
\ ============================================================
\ Required by: scan.fs, get-token, compile loop

create input-buf INPUT-SIZE allot
variable input-len  0 input-len !
variable input-pos  0 input-pos !

\ ============================================================
\ STRING COMPARISON
\ ============================================================
\ Required by: scan.fs, compile.fs, dispatch.fs

: str= ( addr1 u1 addr2 u2 -- flag )
  rot over <> if 2drop drop false exit then
  dup 0= if 2drop drop true exit then
  0 ?do
    over i + c@ over i + c@ <> if 2drop false unloop exit then
  loop
  2drop true ;

\ ============================================================
\ NUMBER PARSING
\ ============================================================
\ Required by: compile loop

variable num-val
variable num-neg

: parse-number ( addr u -- n true | false )
  0 num-val !
  0 num-neg !
  dup 0= if 2drop false exit then
  over c@ [char] - = if
    1 num-neg !
    1 /string
  then
  dup 0= if 2drop false exit then
  over c@ [char] $ = if
    \ Hex number
    1 /string
    dup 0= if 2drop false exit then
    begin
      dup 0> while
      over c@
      dup [char] 0 >= over [char] 9 <= and if
        [char] 0 - num-val @ 16 * + num-val !
      else dup [char] a >= over [char] f <= and if
        [char] a - 10 + num-val @ 16 * + num-val !
      else dup [char] A >= over [char] F <= and if
        [char] A - 10 + num-val @ 16 * + num-val !
      else
        drop 2drop false exit
      then then then
      1 /string
    repeat
  else
    \ Decimal number
    begin
      dup 0> while
      over c@
      dup [char] 0 >= over [char] 9 <= and if
        [char] 0 - num-val @ 10 * + num-val !
      else
        drop 2drop false exit
      then
      1 /string
    repeat
  then
  2drop
  num-val @
  num-neg @ if negate then
  true ;

\ ============================================================
\ TOKENIZER
\ ============================================================
\ Required by: scan.fs, compile loop

: skip-ws ( -- )
  begin
    input-pos @ input-len @ >= if exit then
    input-buf input-pos @ + c@
    dup 32 <= swap 0 > and if
      1 input-pos +!
    else
      exit
    then
  again ;
: skip-line ( -- )
  begin
    input-pos @ input-len @ >= if exit then
    input-buf input-pos @ + c@ 10 = if
      1 input-pos +! exit
    then
    1 input-pos +!
  again ;

: get-token ( -- addr u | 0 0 )
  skip-ws
  input-pos @ input-len @ >= if 0 0 exit then
  \ Skip line comments
  input-buf input-pos @ + c@ 92 = if  \ backslash
    skip-line
    recurse exit
  then
  \ Skip paren comments
  input-buf input-pos @ + c@ 40 = if  \ open paren
    input-pos @ 1+ input-len @ < if
      input-buf input-pos @ 1+ + c@ 32 <= if
        1 input-pos +!
        begin
          input-pos @ input-len @ >= if 0 0 exit then
          input-buf input-pos @ + c@ 41 = if  \ close paren
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
\ USER WORD DICTIONARY
\ ============================================================
\ Required by: compile loop for user-defined words

create dict-buf DICT-SIZE 32 * allot   \ 32 bytes per entry
variable dict-count  0 dict-count !

: dict-entry ( -- addr ) dict-buf dict-count @ 32 * + ;
: dict-name ( entry -- addr ) ;
: dict-addr ( entry -- addr ) 24 + ;
: dict-flags ( entry -- addr ) 28 + ;

: dict-name= ( addr u entry -- flag )
  >r
  dup 23 > if 2drop r> drop false exit then
  r@ over + c@ 0 <> if r> drop 2drop false exit then
  r> swap 0 ?do
    2dup i + c@ swap i + c@ <> if 2drop false unloop exit then
  loop
  2drop true ;

: dict-add ( addr u -- )
  dict-count @ DICT-SIZE >= if 2drop exit then
  dict-entry >r
  r@ 24 0 fill
  dup 23 > if drop 23 then
  r@ swap move
  code-here r@ dict-addr !
  0 r> dict-flags !
  1 dict-count +! ;

: dict-find ( addr u -- entry | 0 )
  dict-count @ 0 ?do
    2dup dict-buf i 32 * + dict-name=
    if 2drop dict-buf i 32 * + unloop exit then
  loop
  2drop 0 ;

\ ============================================================
\ DATA SEGMENT CONSTANTS
\ ============================================================

$800000 constant DATA-BASE

\ Runtime addresses - defined early so prims.fs can use them
DATA-BASE 4256 + constant rt-word-buf    \ 256-byte buffer for null-terminated paths
262144 constant SLURP-SIZE               \ 256KB slurp buffer
DATA-BASE 45312 + constant rt-argc
DATA-BASE 45320 + constant rt-argv
DATA-BASE 45328 + constant rt-slurp-buf  \ slurp-file buffer (SLURP-SIZE bytes)

\ ============================================================
\ LOAD SHANNON MODULES
\ ============================================================
\ Order matters: each layer depends on previous layers.

\ --- Architecture-specific layers (arch/x86/ or arch/arm64/) ---
include compiler/shannon/arch/x86/asm.fs       \ Layer 0: instruction encoding
include compiler/shannon/arch/x86/stack.fs     \ Layer 1: register-mapped stack
include compiler/shannon/arch/x86/prims.fs     \ Layer 2a: primitive codegen
include compiler/shannon/arch/x86/control.fs   \ Layer 2b: control flow codegen
include compiler/shannon/arch/x86/rstack.fs    \ Layer 2c: return stack codegen
include compiler/shannon/arch/x86/io.fs        \ Layer 2d: I/O codegen
\ --- Architecture-independent layers ---
include compiler/shannon/opt-fold.fs  \ Layer 3a: constant folding
include compiler/shannon/opt-fuse.fs  \ Layer 3b: literal fusion
include compiler/shannon/opt-swap.fs  \ Layer 3c: swap elimination
include compiler/shannon/scan.fs      \ Pass 1: word metadata scanner
include compiler/shannon/dispatch.fs  \ Builtin dispatch table
\ --- Architecture-specific binary format ---
include compiler/shannon/arch/x86/elf.fs       \ ELF binary generation
\ --- Mixed layers (arch-specific bytes inline, port incrementally) ---
include compiler/shannon/defs.fs      \ Data definitions (variable, constant, create)
include compiler/shannon/strings.fs   \ String literals (s", .")
include compiler/shannon/compile.fs   \ Layer 4: compilation orchestration

\ ============================================================
\ PROLOGUE/EPILOGUE GENERATION
\ ============================================================

variable start-jmp

: gen-prologue ( -- )
  \ Save argc and argv from Linux stack
  $48 c, $8b c, $04 c, $24 c,              \ mov rax, [rsp]  (argc)
  $48 c, $89 c, $04 c, $25 c, rt-argc d,   \ mov [rt-argc], rax
  $48 c, $8d c, $44 c, $24 c, 8 c,         \ lea rax, [rsp+8]  (argv)
  $48 c, $89 c, $04 c, $25 c, rt-argv d,   \ mov [rt-argv], rax
  \ Set up data and return stacks
  $49 c, $bf c, $800000 $100000 + q,       \ r15 = data stack at 0x900000
  $48 c, $bd c, $800000 $180000 + q,       \ rbp = return stack at 0x980000
  $e8 c, code-here start-jmp ! 0 d, ;      \ call main (patched later)

: patch-start ( -- )
  \ Patch the jump to main - look up main in dictionary
  s" main" dict-find dup 0= if
    drop ." Error: no main found" cr 1 throw
  then
  dict-addr @                     \ Get main's code address
  start-jmp @ 4 + -               \ Calculate displacement
  start-jmp @ code-buf + d! ;

: gen-epilogue ( -- )
  \ exit(0) syscall
  $b8 c, 60 d,    \ mov eax, 60 (SYS_exit)
  $31 c, $ff c,   \ xor edi, edi
  $0f c, $05 c, ; \ syscall

\ ============================================================
\ MAIN COMPILATION LOOP (Pass 2)
\ ============================================================

: compile-token ( addr u -- )
  \ Try number first
  2dup parse-number if
    nip nip compile-number exit
  then
  \ Try optimized builtins
  2dup try-fold-binary if 2drop exit then
  2dup try-fold-unary if 2drop exit then
  2dup try-stack-op if 2drop exit then
  2dup try-compare if 2drop exit then
  2dup try-memory if 2drop exit then
  2dup try-control if 2drop exit then
  2dup try-rstack if 2drop exit then
  2dup try-io if 2drop exit then
  2dup try-strings if 2drop exit then
  2dup try-special if 2drop exit then
  \ Try user dictionary
  2dup dict-find ?dup if          ( addr u entry )
    \ Check $800 flag - inline variable/constant/create stubs
    dup dict-flags @ $FFFFFFFF and $800 and if
      \ Inline: push value from stub's mov rax,imm64 instruction
      dict-addr @ $FFFFFFFF and 2 + code-buf + @   \ get imm64 value (after 48 b8)
      ct-push                        \ push to compile-time stack
      2drop exit
    then
    >r                             \ save entry, ( addr u )
    info-find ?dup if              ( info-entry|0 )
      24 + c@                      ( nargs )
    else
      0                            ( default: variables/stubs take 0 args )
    then
    r> swap >r                     \ ( entry ) nargs on rstack
    flush-all
    \ Save extra stack items beyond what callee needs
    stack-depth @ r@ - 1 >= if $53 c, then   \ push rbx if extra
    stack-depth @ r@ - 2 >= if $51 c, then   \ push rcx if extra
    dict-addr @ code-here 5 + -    \ relative offset
    $e8 c, d,                       \ call rel32
    \ Restore after call
    stack-depth @ r@ - 2 >= if $59 c, then   \ pop rcx
    stack-depth @ r> - 1 >= if $5b c, then   \ pop rbx
    exit
  then
  \ Unknown word - error
  ." Unknown: " type cr
  1 throw ;

: compile-colon ( -- )
  \ Parse word name, compile body until ;
  get-token dup 0= if 2drop exit then
  2dup dict-add
  \ Track current word address for recurse
  dict-buf dict-count @ 1- 32 * + dict-addr @ current-word-addr !
  reset-compile-state
  \ Look up word in info-buf to get nargs for initial stack-depth
  info-find ?dup if
    24 + c@                         \ get nargs from info entry
  else
    1                               \ default to 1 if not found
  then
  stack-depth !
  begin
    get-token
    dup 0= if 2drop exit then
    2dup s" ;" str= if
      2drop
      flush-all
      ret
      exit
    then
    compile-token
  again ;

\ ============================================================
\ FILE I/O
\ ============================================================

: load-source ( addr u -- )
  slurp-file
  dup INPUT-SIZE > if
    ." File too large" cr 1 throw
  then
  dup input-len !
  input-buf swap move
  0 input-pos ! ;

\ ============================================================
\ INCLUDE STACK (for nested includes)
\ ============================================================
\ Each level needs: input-buf copy (INPUT-SIZE), input-pos, input-len
\ Support 8 levels of nesting (should be plenty)

\ Forward reference for compile-all (needed for recursive include)
variable 'compile-all  0 'compile-all !

8 constant MAX-INCLUDE-DEPTH
MAX-INCLUDE-DEPTH INPUT-SIZE * constant INCLUDE-STACK-SIZE
create include-stack INCLUDE-STACK-SIZE allot
create include-pos-stack MAX-INCLUDE-DEPTH cells allot
create include-len-stack MAX-INCLUDE-DEPTH cells allot
variable include-depth  0 include-depth !

: include-push ( -- )
  \ Save current input state to include stack
  include-depth @ MAX-INCLUDE-DEPTH >= if
    ." Include nesting too deep" cr 1 throw
  then
  \ Save input-buf contents
  input-buf
  include-stack include-depth @ INPUT-SIZE * +
  input-len @ move
  \ Save input-pos and input-len
  input-pos @ include-pos-stack include-depth @ cells + !
  input-len @ include-len-stack include-depth @ cells + !
  1 include-depth +! ;

: include-pop ( -- )
  \ Restore previous input state from include stack
  include-depth @ 0= if exit then
  -1 include-depth +!
  \ Restore input-buf contents
  include-stack include-depth @ INPUT-SIZE * +
  input-buf
  include-len-stack include-depth @ cells + @
  move
  \ Restore input-pos and input-len
  include-pos-stack include-depth @ cells + @ input-pos !
  include-len-stack include-depth @ cells + @ input-len ! ;

: compile-include ( -- )
  \ Get filename token
  get-token dup 0= if 2drop exit then
  \ Save current input state
  include-push
  \ Load the new file (overwrites input-buf)
  load-source
  \ Process it with compile-all (via forward reference)
  'compile-all @ execute
  \ Restore previous input state
  include-pop ;

: compile-all ( -- )
  begin
    get-token dup 0= if 2drop exit then
    2dup s" :" str= if
      2drop compile-colon
    else 2dup s" include" str= if
      2drop compile-include
    else 2dup s" variable" str= if
      2drop compile-variable
    else 2dup s" constant" str= if
      2drop compile-constant
    else 2dup s" create" str= if
      2drop compile-create
    else 2dup s" allot" str= if
      2drop compile-allot
    else
      \ Check if it's a number (for constant values)
      2dup parse-number if
        nip nip ct-push
      else
        \ Check if it's a constant ($800 flag) - push its value
        2dup dict-find ?dup if
          dup dict-flags @ $FFFFFFFF and $800 and if
            \ Get imm64 value from stub (offset +2 after 48 b8)
            dict-addr @ $FFFFFFFF and 2 + code-buf + @
            ct-push
            2drop
          else
            drop 2drop  \ Not a constant, skip
          then
        else
          2drop  \ Unknown token, skip
        then
      then
    then then then then then then
  again ;

\ Initialize forward reference
' compile-all 'compile-all !

\ ============================================================
\ MAIN ENTRY POINT
\ ============================================================

create src-name 256 allot
variable src-len
create out-name 256 allot
variable out-len

: save-src ( addr u -- ) dup src-len ! src-name swap move ;
: save-out ( addr u -- ) dup out-len ! out-name swap move ;
: get-src ( -- addr u ) src-name src-len @ ;
: get-out ( -- addr u ) out-name out-len @ ;

: compile-file ( src-addr src-u out-addr out-u -- )
  2swap load-source
  \ Pass 1: Scan all definitions
  scan-all
  \ Reset for Pass 2
  0 input-pos !
  0 code-pos !
  0 dict-count !
  0 stack-depth !
  ct-reset
  clear-swap
  \ Generate prologue (ends with call main)
  gen-prologue
  \ Generate epilogue right after call - this is where main returns to
  gen-epilogue
  \ Pass 2: Compile all definitions (after epilogue)
  compile-all
  \ Patch call to point to main
  patch-start
  \ Generate ELF header and write output
  elf-header
  write-elf ;

: usage ( -- )
  ." Usage: fifth compiler/shannon/main.fs <input.fs> <output>" cr
  bye ;

: main ( -- )
  argc 4 < if usage then
  2 argv save-src
  3 argv save-out
  get-src get-out compile-file
  bye ;

main

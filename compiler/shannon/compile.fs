\ compile.fs - Main Compilation Orchestration (Shannon Layer 4)
\ Ties together all Shannon layers into the main compilation loop.
\
\ This module makes optimization DECISIONS but delegates work to:
\   - scan.fs     : info-buf lookup (Pass 1 data)
\   - stack.fs    : stack-depth tracking, emit-lit
\   - prims.fs    : pure codegen (emit-*)
\   - opt-fold.fs : constant folding (ct-stack)
\   - opt-fuse.fs : literal fusion (fuse-*)
\   - opt-swap.fs : swap elimination (mark-swap, flush-swap)
\
\ The compilation strategy:
\   1. Numbers -> ct-push (accumulate for folding)
\   2. Foldable binary ops with ct-depth>=2 -> fold
\   3. Fusable ops with ct-depth=1 -> fuse (immediate instruction)
\   4. Otherwise -> ct-flush, emit primitive
\
\ Requires: All Shannon modules loaded, plus:
\   - input-buf, input-pos, input-len, get-token
\   - dict-buf, dict-find, dict-add, dict-addr, dict-flags
\   - parse-number, str=
\   - code-buf, code-pos, code-here, c,, d,, q,

\ ============================================================
\ COMPILATION STATE (minimal, most state in submodules)
\ ============================================================

variable state         0 state !       \ 0=interpret, 1=compile
variable dup-pending   0 dup-pending ! \ Deferred dup for dup 0= fusion
variable cmp-pending   0 cmp-pending ! \ Deferred comparison (1=0=, 2=0>, 3=0<)

\ ============================================================
\ STATE RESET
\ ============================================================

: reset-compile-state ( -- )
  \ Called at start of each word definition
  ct-reset
  clear-swap
  0 dup-pending !
  0 cmp-pending ! ;

\ ============================================================
\ PENDING OPERATION FLUSH
\ ============================================================

: flush-pending ( -- )
  \ Emit deferred dup if any
  dup-pending @ if emit-dup 0 dup-pending ! then ;

: flush-cmp ( -- )
  \ Emit deferred comparison if any
  cmp-pending @ ?dup if
    0 cmp-pending !
    dup 1 = if drop emit-0= else
    dup 2 = if drop emit-0> else
    dup 3 = if drop emit-0< else
    drop then then then
  then ;

: flush-all ( -- )
  \ Flush all pending operations before non-optimizable ops
  flush-swap ct-flush flush-pending flush-cmp ;

\ ============================================================
\ ARITHMETIC COMPILATION - Fold/Fuse/Emit Decision
\ ============================================================

: compile-+ ( -- )
  \ ( x y -- x+y ) with optimization
  flush-cmp
  \ Commutative: absorb pending swap
  ct-depth@ 0= if clear-swap else flush-swap then
  ct-depth@ 2 >= if
    fold-add                        \ Pure fold
  else ct-depth@ 1 = if
    flush-pending fuse-add          \ Fuse: add-imm
  else
    flush-pending emit-add          \ Runtime add
  then then ;

: compile-- ( -- )
  \ ( x y -- x-y ) - non-commutative, handle swap specially
  flush-cmp
  swap-pending? if
    ct-depth@ 0= if
      clear-swap flush-pending
      \ swap sub = rsub (reverse subtract)
      nos tos sub-rr                \ tos = tos - nos (reversed)
      pop-nos-val
    else
      flush-swap
      ct-depth@ 2 >= if fold-sub else
      ct-depth@ 1 = if flush-pending fuse-sub else
      flush-pending emit-sub then then
    then
  else
    ct-depth@ 2 >= if fold-sub else
    ct-depth@ 1 = if flush-pending fuse-sub else
    flush-pending emit-sub then then
  then ;

: compile-* ( -- )
  \ ( x y -- x*y ) with strength reduction
  flush-cmp
  ct-depth@ 0= if clear-swap else flush-swap then
  ct-depth@ 2 >= if
    fold-mul
  else ct-depth@ 1 = if
    flush-pending fuse-mul          \ Handles power-of-2 -> shift
  else
    flush-pending emit-mul
  then then ;

: compile-/ ( -- )
  \ ( x y -- x/y ) non-commutative division
  flush-cmp flush-swap
  ct-depth@ 2 >= if
    fold-div
  else
    flush-pending emit-/
  then ;

: compile-mod ( -- )
  \ ( n d -- n mod d ) modulo
  flush-cmp flush-swap
  ct-depth@ 2 >= if
    fold-mod
  else
    flush-pending emit-mod
  then ;

: compile-/mod ( -- )
  \ ( n d -- rem quot ) combined division
  flush-all emit-/mod ;

: compile-lshift ( -- )
  \ ( x n -- x<<n ) left shift
  flush-cmp flush-swap
  ct-depth@ 2 >= if
    fold-lshift
  else ct-depth@ 1 = if
    flush-pending fuse-lshift
  else
    flush-pending emit-lshift
  then then ;

: compile-rshift ( -- )
  \ ( x n -- x>>n ) logical right shift
  flush-cmp flush-swap
  ct-depth@ 2 >= if
    fold-rshift
  else ct-depth@ 1 = if
    flush-pending fuse-rshift
  else
    flush-pending emit-rshift
  then then ;

: compile-min ( -- )
  \ ( a b -- min ) signed minimum, commutative
  flush-cmp
  ct-depth@ 0= if clear-swap else flush-swap then
  ct-depth@ 2 >= if
    ct-pop ct-pop min ct-push
  else
    flush-pending emit-min
  then ;

: compile-max ( -- )
  \ ( a b -- max ) signed maximum, commutative
  flush-cmp
  ct-depth@ 0= if clear-swap else flush-swap then
  ct-depth@ 2 >= if
    ct-pop ct-pop max ct-push
  else
    flush-pending emit-max
  then ;

: compile-and ( -- )
  \ ( x y -- x&y ) commutative
  flush-cmp
  ct-depth@ 0= if clear-swap else flush-swap then
  ct-depth@ 2 >= if
    fold-and
  else ct-depth@ 1 = if
    flush-pending fuse-and
  else
    flush-pending emit-and
  then then ;

: compile-or ( -- )
  \ ( x y -- x|y ) commutative
  flush-cmp
  ct-depth@ 0= if clear-swap else flush-swap then
  ct-depth@ 2 >= if
    fold-or
  else ct-depth@ 1 = if
    flush-pending fuse-or
  else
    flush-pending emit-or
  then then ;

: compile-xor ( -- )
  \ ( x y -- x^y ) commutative
  flush-cmp
  ct-depth@ 0= if clear-swap else flush-swap then
  ct-depth@ 2 >= if
    fold-xor
  else ct-depth@ 1 = if
    flush-pending fuse-xor
  else
    flush-pending emit-xor
  then then ;

\ ============================================================
\ UNARY ARITHMETIC - Fold or Emit
\ ============================================================

: compile-negate ( -- )
  flush-cmp
  \ swap negate operates on NOS (rbx)
  swap-pending? if
    ct-depth@ 0> if ct-pop negate ct-push else
    $48 c, $f7 c, $db c, then       \ neg rbx
  else
    ct-depth@ 0> if fold-negate else emit-negate then
  then ;

: compile-1+ ( -- )
  flush-cmp flush-pending
  \ swap 1+ operates on NOS
  swap-pending? if
    ct-depth@ 0> if ct-pop 1+ ct-push else
    $48 c, $ff c, $c3 c, then       \ inc rbx
  else
    ct-depth@ 0> if fold-1+ else emit-1+ then
  then ;

: compile-1- ( -- )
  flush-cmp flush-pending
  swap-pending? if
    ct-depth@ 0> if ct-pop 1- ct-push else
    $48 c, $ff c, $cb c, then       \ dec rbx
  else
    ct-depth@ 0> if fold-1- else emit-1- then
  then ;

: compile-invert ( -- )
  flush-swap
  ct-depth@ 0> if fold-invert else emit-invert then ;

: compile-2* ( -- )
  flush-swap
  ct-depth@ 0> if fold-2* else
  1 emit-lshift-imm then ;          \ shl rax, 1 via add rax,rax

: compile-2/ ( -- )
  flush-swap
  ct-depth@ 0> if fold-2/ else
  1 tos sar-ri then ;               \ sar rax, 1

: compile-cells ( -- )
  flush-swap
  ct-depth@ 0> if ct-pop 8 * ct-push else emit-cells then ;

: compile-abs ( -- )
  \ ( n -- |n| ) absolute value
  flush-swap
  ct-depth@ 0> if ct-pop abs ct-push else emit-abs then ;

\ ============================================================
\ STACK OPERATIONS
\ ============================================================

: compile-dup ( -- )
  flush-swap ct-flush
  1 dup-pending ! ;                 \ Defer for 0= fusion

: compile-drop ( -- )
  flush-swap
  ct-depth@ 0> if
    ct-pop drop                     \ Discard compile-time constant
  else
    ct-flush emit-drop
  then ;

: compile-swap ( -- )
  ct-flush
  cancel-swap ;                     \ Toggle pending swap

: compile-over ( -- )
  flush-all emit-over ;

: compile-rot ( -- )
  flush-all emit-rot ;

: compile-nip ( -- )
  flush-all emit-nip ;

: compile-tuck ( -- )
  flush-all emit-tuck ;

: compile-2dup ( -- )
  flush-all emit-2dup ;

: compile-2drop ( -- )
  flush-all emit-2drop ;

: compile-2swap ( -- )
  flush-all emit-2swap ;

: compile-2over ( -- )
  flush-all emit-2over ;

: compile--rot ( -- )
  flush-all emit--rot ;

: compile-?dup ( -- )
  flush-all emit-?dup ;

: compile-depth ( -- )
  flush-all emit-depth ;

: compile-pick ( -- )
  flush-all emit-pick ;

\ ============================================================
\ COMPARISON OPERATIONS
\ ============================================================

: compile-= ( -- )
  flush-all emit-= ;

: compile-< ( -- )
  flush-all emit-< ;

: compile-> ( -- )
  flush-all emit-> ;

: compile-0= ( -- )
  \ TODO: fused dup 0= if optimization disabled for now
  flush-all emit-0= ;

: compile-0< ( -- )
  flush-all emit-0< ;

: compile-0> ( -- )
  flush-all emit-0> ;

: compile-<> ( -- )
  flush-all emit-<> ;

: compile-u< ( -- )
  flush-all emit-u< ;

: compile-0<> ( -- )
  flush-all emit-0<> ;

: compile-<= ( -- )
  flush-all emit-<= ;

: compile->= ( -- )
  flush-all emit->= ;

\ ============================================================
\ MEMORY OPERATIONS
\ ============================================================

: compile-@ ( -- )
  flush-swap ct-flush emit-@ ;

: compile-! ( -- )
  flush-all emit-! ;

: compile-c@ ( -- )
  flush-swap ct-flush emit-c@ ;

: compile-c! ( -- )
  flush-all emit-c! ;

: compile-+! ( -- )
  flush-all emit-+! ;

: compile-fill ( -- )
  flush-all emit-fill ;

: compile-move ( -- )
  flush-all emit-move ;

: compile-/string ( -- )
  flush-all emit-/string ;

\ ============================================================
\ CONTROL FLOW SUPPORT
\ ============================================================
\ Control flow words need careful handling of pending operations.
\ The actual control flow implementation (IF/THEN/ELSE/BEGIN/etc)
\ remains in the main compiler since it needs access to:
\   - cf-stack (control flow stack)
\   - gen-if, gen-then, gen-else, etc.
\   - Forward reference patching
\
\ This module provides the flush interface these words need.

: pre-control-flow ( -- )
  \ Called before any control flow word
  flush-all ;

\ ============================================================
\ NUMBER HANDLING
\ ============================================================

: compile-number ( n -- )
  \ Push number to compile-time stack for potential folding
  dup-pending @ if emit-dup 0 dup-pending ! then
  ct-push ;

\ ============================================================
\ BUILTIN DISPATCH TABLE
\ ============================================================
\ Maps builtin names to their compile handlers.
\ Returns true if handled, false if not a builtin.
\
\ Note: The full builtin table is large (~100 words). This module
\ shows the pattern; the complete dispatch lives in the main compiler
\ until full extraction is complete.

\ String comparison helper (assumes str= is available)
\ : builtin? ( addr u c-addr -- addr u flag )
\   count 2over str= ;

\ ============================================================
\ MAIN COMPILATION INTERFACE
\ ============================================================
\ These are the entry points called by the main compiler loop.

: try-fold-binary ( addr u -- handled? )
  \ Try to compile as a foldable binary operation
  \ Returns true if handled
  2dup s" +" str= if 2drop compile-+ true exit then
  2dup s" -" str= if 2drop compile-- true exit then
  2dup s" *" str= if 2drop compile-* true exit then
  2dup s" /" str= if 2drop compile-/ true exit then
  2dup s" mod" str= if 2drop compile-mod true exit then
  2dup s" /mod" str= if 2drop compile-/mod true exit then
  2dup s" and" str= if 2drop compile-and true exit then
  2dup s" or" str= if 2drop compile-or true exit then
  2dup s" xor" str= if 2drop compile-xor true exit then
  2dup s" lshift" str= if 2drop compile-lshift true exit then
  2dup s" rshift" str= if 2drop compile-rshift true exit then
  2dup s" min" str= if 2drop compile-min true exit then
  2dup s" max" str= if 2drop compile-max true exit then
  2drop false ;

: try-fold-unary ( addr u -- handled? )
  \ Try to compile as a foldable unary operation
  2dup s" negate" str= if 2drop compile-negate true exit then
  2dup s" 1+" str= if 2drop compile-1+ true exit then
  2dup s" 1-" str= if 2drop compile-1- true exit then
  2dup s" invert" str= if 2drop compile-invert true exit then
  2dup s" 2*" str= if 2drop compile-2* true exit then
  2dup s" 2/" str= if 2drop compile-2/ true exit then
  2dup s" abs" str= if 2drop compile-abs true exit then
  2dup s" cells" str= if 2drop compile-cells true exit then
  2drop false ;

: try-stack-op ( addr u -- handled? )
  \ Try to compile as a stack operation
  2dup s" dup" str= if 2drop compile-dup true exit then
  2dup s" drop" str= if 2drop compile-drop true exit then
  2dup s" swap" str= if 2drop compile-swap true exit then
  2dup s" over" str= if 2drop compile-over true exit then
  2dup s" rot" str= if 2drop compile-rot true exit then
  2dup s" nip" str= if 2drop compile-nip true exit then
  2dup s" tuck" str= if 2drop compile-tuck true exit then
  2dup s" 2dup" str= if 2drop compile-2dup true exit then
  2dup s" 2drop" str= if 2drop compile-2drop true exit then
  2dup s" 2swap" str= if 2drop compile-2swap true exit then
  2dup s" 2over" str= if 2drop compile-2over true exit then
  2dup s" -rot" str= if 2drop compile--rot true exit then
  2dup s" ?dup" str= if 2drop compile-?dup true exit then
  2dup s" depth" str= if 2drop compile-depth true exit then
  2dup s" pick" str= if 2drop compile-pick true exit then
  2drop false ;

: try-compare ( addr u -- handled? )
  \ Try to compile as a comparison
  2dup s" =" str= if 2drop compile-= true exit then
  2dup s" <" str= if 2drop compile-< true exit then
  2dup s" >" str= if 2drop compile-> true exit then
  2dup s" 0=" str= if 2drop compile-0= true exit then
  2dup s" 0<" str= if 2drop compile-0< true exit then
  2dup s" 0>" str= if 2drop compile-0> true exit then
  2dup s" <>" str= if 2drop compile-<> true exit then
  2dup s" u<" str= if 2drop compile-u< true exit then
  2dup s" 0<>" str= if 2drop compile-0<> true exit then
  2dup s" <=" str= if 2drop compile-<= true exit then
  2dup s" >=" str= if 2drop compile->= true exit then
  2drop false ;

: try-memory ( addr u -- handled? )
  \ Try to compile as a memory operation
  2dup s" @" str= if 2drop compile-@ true exit then
  2dup s" !" str= if 2drop compile-! true exit then
  2dup s" c@" str= if 2drop compile-c@ true exit then
  2dup s" c!" str= if 2drop compile-c! true exit then
  2dup s" +!" str= if 2drop compile-+! true exit then
  2dup s" fill" str= if 2drop compile-fill true exit then
  2dup s" move" str= if 2drop compile-move true exit then
  2dup s" /string" str= if 2drop compile-/string true exit then
  2drop false ;

\ ============================================================
\ CONTROL FLOW COMPILATION
\ ============================================================
\ Uses cf-stack from control.fs to manage forward references

: compile-if ( -- ) flush-all gen-if cf-push ;
: compile-then ( -- ) cf-pop gen-then ;
: compile-else ( -- ) cf-pop gen-else cf-push ;
: compile-begin ( -- ) flush-all gen-begin cf-push ;
: compile-until ( -- ) flush-all cf-pop gen-until ;
: compile-again ( -- ) flush-all cf-pop gen-again ;
: compile-while ( -- ) flush-all gen-if cf-push ;
: compile-repeat ( -- ) cf-pop cf-pop gen-again gen-then ;
: compile-do ( -- ) flush-all gen-do cf-push cf-push ;
: compile-?do ( -- ) flush-all gen-?do cf-push cf-push ;
: compile-loop ( -- ) cf-pop cf-pop gen-loop ;
: compile-+loop ( -- ) flush-all cf-pop cf-pop gen-+loop ;
: compile-i ( -- ) flush-all gen-i ;
: compile-j ( -- ) flush-all gen-j ;
: compile-unloop ( -- ) gen-unloop ;
: compile-leave ( -- ) flush-all gen-leave ;

\ Fused compare+branch
: compile-0=if ( -- ) flush-all gen-0=if ;
: compile-0<if ( -- ) flush-all gen-0<if ;
: compile-<if ( -- ) flush-all gen-<if ;
: compile->if ( -- ) flush-all gen->if ;
: compile-=if ( -- ) flush-all gen-=if ;

: try-control ( addr u -- handled? )
  2dup s" if" str= if 2drop compile-if true exit then
  2dup s" then" str= if 2drop compile-then true exit then
  2dup s" else" str= if 2drop compile-else true exit then
  2dup s" begin" str= if 2drop compile-begin true exit then
  2dup s" until" str= if 2drop compile-until true exit then
  2dup s" again" str= if 2drop compile-again true exit then
  2dup s" while" str= if 2drop compile-while true exit then
  2dup s" repeat" str= if 2drop compile-repeat true exit then
  2dup s" do" str= if 2drop compile-do true exit then
  2dup s" ?do" str= if 2drop compile-?do true exit then
  2dup s" loop" str= if 2drop compile-loop true exit then
  2dup s" +loop" str= if 2drop compile-+loop true exit then
  2dup s" i" str= if 2drop compile-i true exit then
  2dup s" j" str= if 2drop compile-j true exit then
  2dup s" unloop" str= if 2drop compile-unloop true exit then
  2dup s" leave" str= if 2drop compile-leave true exit then
  \ Fused variants
  2dup s" 0=if" str= if 2drop compile-0=if true exit then
  2dup s" 0<if" str= if 2drop compile-0<if true exit then
  2dup s" <if" str= if 2drop compile-<if true exit then
  2dup s" >if" str= if 2drop compile->if true exit then
  2dup s" =if" str= if 2drop compile-=if true exit then
  2drop false ;

\ ============================================================
\ RETURN STACK COMPILATION
\ ============================================================

: compile->r ( -- ) flush-all gen->r ;
: compile-r> ( -- ) flush-all gen-r> ;
: compile-r@ ( -- ) flush-all gen-r@ ;
: compile-2>r ( -- ) flush-all gen-2>r ;
: compile-2r> ( -- ) flush-all gen-2r> ;
: compile-2r@ ( -- ) flush-all gen-2r@ ;

: try-rstack ( addr u -- handled? )
  2dup s" >r" str= if 2drop compile->r true exit then
  2dup s" r>" str= if 2drop compile-r> true exit then
  2dup s" r@" str= if 2drop compile-r@ true exit then
  2dup s" 2>r" str= if 2drop compile-2>r true exit then
  2dup s" 2r>" str= if 2drop compile-2r> true exit then
  2dup s" 2r@" str= if 2drop compile-2r@ true exit then
  2drop false ;

\ ============================================================
\ I/O COMPILATION
\ ============================================================

: compile-emit ( -- ) flush-all gen-emit ;
: compile-cr ( -- ) flush-all gen-cr ;
: compile-space ( -- ) flush-all gen-space ;
: compile-type ( -- ) flush-all gen-type ;
: compile-dot ( -- ) flush-all gen-dot ;
: compile-key ( -- ) flush-all gen-key ;
: compile-write-file ( -- ) flush-all emit-write-file ;
: compile-close-file ( -- ) flush-all emit-close-file ;
: compile-argc ( -- ) flush-all emit-argc ;
: compile-argv ( -- ) flush-all emit-argv ;
: compile-slurp-file ( -- ) flush-all emit-slurp-file ;
: compile-w/o ( -- ) flush-all emit-w/o ;
: compile-r/o ( -- ) flush-all emit-r/o ;
: compile-r/w ( -- ) flush-all emit-r/w ;
: compile-create-file ( -- ) flush-all gen-create-file ;

: try-io ( addr u -- handled? )
  2dup s" emit" str= if 2drop compile-emit true exit then
  2dup s" cr" str= if 2drop compile-cr true exit then
  2dup s" space" str= if 2drop compile-space true exit then
  2dup s" type" str= if 2drop compile-type true exit then
  2dup s" ." str= if 2drop compile-dot true exit then
  2dup s" key" str= if 2drop compile-key true exit then
  2dup s" write-file" str= if 2drop compile-write-file true exit then
  2dup s" close-file" str= if 2drop compile-close-file true exit then
  2dup s" argc" str= if 2drop compile-argc true exit then
  2dup s" argv" str= if 2drop compile-argv true exit then
  2dup s" slurp-file" str= if 2drop compile-slurp-file true exit then
  2dup s" w/o" str= if 2drop compile-w/o true exit then
  2dup s" r/o" str= if 2drop compile-r/o true exit then
  2dup s" r/w" str= if 2drop compile-r/w true exit then
  2dup s" create-file" str= if 2drop compile-create-file true exit then
  2drop false ;

\ ============================================================
\ STRING LITERALS
\ ============================================================

: compile-s" ( -- ) flush-all gen-s" ;
: compile-." ( -- ) flush-all gen-dotquote ;

: try-strings ( addr u -- handled? )
  2dup s\" s\"" str= if 2drop compile-s" true exit then
  2dup s\" .\"" str= if 2drop compile-." true exit then
  2dup s" [char]" str= if 2drop compile-bracket-char true exit then
  2dup s" char" str= if 2drop compile-char true exit then
  2drop false ;

\ ============================================================
\ SPECIAL WORDS (recurse, exit)
\ ============================================================

: compile-recurse ( -- ) flush-all gen-recurse ;
: compile-exit ( -- ) flush-all ret ;
: compile-throw ( -- ) flush-all emit-throw ;
: compile-bye ( -- ) flush-all emit-bye ;

: compile-tick ( -- )
  \ ' at compile time: get next token, look up in dict, push code address as literal
  get-token dup 0= if 2drop exit then
  dict-find ?dup if
    flush-all
    dict-addr @ emit-lit           \ push the XT (code address)
  else
    ." ' unknown word" cr 1 throw
  then ;

: compile-execute ( -- )
  \ execute: call the XT on TOS
  flush-all
  $ff c, $d0 c,                    \ call rax
  pop-val ;

: compile-literal ( -- )
  \ literal: take value from ct-stack, compile as literal
  ct-depth@ 0> if
    ct-pop emit-lit
  else
    ." literal: nothing on compile-time stack" cr 1 throw
  then ;

: compile-bracket-tick ( -- )
  \ ['] at compile time: identical to ' - get next token, look up, emit XT as literal
  get-token dup 0= if 2drop exit then
  dict-find ?dup if
    flush-all
    dict-addr @ emit-lit           \ push the XT (code address)
  else
    ." ['] unknown word" cr 1 throw
  then ;

: try-special ( addr u -- handled? )
  2dup s" recurse" str= if 2drop compile-recurse true exit then
  2dup s" exit" str= if 2drop compile-exit true exit then
  2dup s" throw" str= if 2drop compile-throw true exit then
  2dup s" bye" str= if 2drop compile-bye true exit then
  2dup s" '" str= if 2drop compile-tick true exit then
  2dup s" [']" str= if 2drop compile-bracket-tick true exit then
  2dup s" execute" str= if 2drop compile-execute true exit then
  2dup s" literal" str= if 2drop compile-literal true exit then
  2drop false ;

\ ============================================================
\ OPTIMIZATION SUMMARY
\ ============================================================
\ This orchestration layer implements these optimizations:
\
\ 1. CONSTANT FOLDING (ct-stack)
\    - Binary ops with 2+ constants: fold at compile time
\    - Unary ops with 1+ constants: fold at compile time
\    - Example: 2 3 + -> 5 (no runtime code)
\
\ 2. LITERAL FUSION (fuse-*)
\    - Binary ops with 1 constant: emit immediate instruction
\    - Example: x 5 + -> add rax, 5 (not: mov rbx,5; add rax,rbx)
\    - Strength reduction: x 8 * -> shl rax, 3
\
\ 3. SWAP ELIMINATION (swap-pending)
\    - Commutative ops absorb pending swap
\    - Example: swap + -> + (operand order doesn't matter)
\    - swap swap cancels
\
\ 4. DUP FUSION (dup-pending)
\    - dup 0= -> test+setz without materializing dup
\    - Saves a register move
\
\ 5. COMPARISON FUSION (cmp-pending)
\    - dup 0= if -> test + jz (no intermediate flag value)
\
\ The key insight: delay operations until we know what follows.
\ This enables peephole optimization across Forth word boundaries.


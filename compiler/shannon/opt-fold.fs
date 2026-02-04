\ opt-fold.fs - Constant folding optimization (Shannon Layer 3)
\ Owns ct-stack and ct-depth. Evaluates constants at compile time.
\
\ Requires: stack.fs for emit-lit

\ ============================================================
\ STATE (owned by this module)
\ ============================================================

create ct-stack 8 cells allot   \ Compile-time evaluation stack
variable ct-depth               \ Current depth of ct-stack
0 ct-depth !

\ ============================================================
\ PUBLIC INTERFACE - Basic Operations
\ ============================================================

: ct-depth@ ( -- n )
  \ Query compile-time stack depth
  ct-depth @ ;

: ct-push ( n -- )
  \ Push constant onto compile-time stack
  ct-stack ct-depth @ cells + !
  1 ct-depth +! ;

: ct-pop ( -- n )
  \ Pop constant from compile-time stack
  -1 ct-depth +!
  ct-stack ct-depth @ cells + @ ;

: ct-reset ( -- )
  \ Clear compile-time stack (at start of word)
  0 ct-depth ! ;

: ct-flush ( -- )
  \ Emit all pending constants as literals (FIFO order)
  ct-depth @ 0= if exit then
  ct-depth @ 0 do
    ct-stack i cells + @ emit-lit
  loop
  0 ct-depth ! ;

\ ============================================================
\ BINARY FOLDING - Requires ct-depth >= 2
\ ============================================================

: fold-add ( -- )
  \ ct: ( a b -- a+b )
  ct-pop ct-pop + ct-push ;

: fold-sub ( -- )
  \ ct: ( a b -- a-b ) where b is TOS
  ct-pop ct-pop swap - ct-push ;

: fold-mul ( -- )
  \ ct: ( a b -- a*b )
  ct-pop ct-pop * ct-push ;

: fold-div ( -- )
  \ ct: ( a b -- a/b )
  ct-pop ct-pop swap / ct-push ;

: fold-mod ( -- )
  \ ct: ( a b -- a mod b )
  ct-pop ct-pop swap mod ct-push ;

: fold-and ( -- )
  \ ct: ( a b -- a and b )
  ct-pop ct-pop and ct-push ;

: fold-or ( -- )
  \ ct: ( a b -- a or b )
  ct-pop ct-pop or ct-push ;

: fold-xor ( -- )
  \ ct: ( a b -- a xor b )
  ct-pop ct-pop xor ct-push ;

: fold-lshift ( -- )
  \ ct: ( a b -- a << b )
  ct-pop ct-pop swap lshift ct-push ;

: fold-rshift ( -- )
  \ ct: ( a b -- a >> b )
  ct-pop ct-pop swap rshift ct-push ;

\ ============================================================
\ UNARY FOLDING - Requires ct-depth >= 1
\ ============================================================

: fold-negate ( -- )
  \ ct: ( a -- -a )
  ct-pop negate ct-push ;

: fold-invert ( -- )
  \ ct: ( a -- ~a )
  ct-pop invert ct-push ;

: fold-1+ ( -- )
  \ ct: ( a -- a+1 )
  ct-pop 1+ ct-push ;

: fold-1- ( -- )
  \ ct: ( a -- a-1 )
  ct-pop 1- ct-push ;

: fold-2* ( -- )
  \ ct: ( a -- a*2 )
  ct-pop 2 * ct-push ;

: fold-2/ ( -- )
  \ ct: ( a -- a/2 )
  ct-pop 2 / ct-push ;

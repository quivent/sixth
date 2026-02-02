\ boot/core.fs - Sixth Bootstrap
\ Defines high-level words on top of C primitives.
\ Loaded automatically at engine startup.

\ ============================================================
\ Defining Words
\ ============================================================

\ VARIABLE ( "name" -- ) Create a variable with initial value 0
: variable  create 0 , ;

\ CONSTANT ( x "name" -- ) Create a constant
: constant  create , does> @ ;

\ 2CONSTANT ( x1 x2 "name" -- ) Create a double constant
: 2constant  create , , does> dup cell+ @ swap @ ;

\ VALUE ( x "name" -- ) Like constant but mutable with TO
\ Simplified: just use variable semantics
: value  create , does> @ ;

\ DEFER ( "name" -- ) Create a deferred word
: defer  create ['] noop , does> @ execute ;

\ IS ( xt "name" -- ) Set a deferred word
: is  ' >body ! ;

\ ============================================================
\ Stack Utilities
\ ============================================================

\ 2ROT ( x1 x2 x3 x4 x5 x6 -- x3 x4 x5 x6 x1 x2 )
: 2rot  2>r 2swap 2r> 2swap ;

\ ============================================================
\ Memory Utilities
\ ============================================================

\ ERASE ( addr u -- ) Fill memory with zeros
: erase  0 fill ;

\ fib.fs - Iterative fibonacci
: fib ( n -- f ) 0 1 rot 0 do tuck+ loop drop ;
: main ( -- ) 35 fib . cr ;

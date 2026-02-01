\ fib.fs - Iterative fibonacci (1B iterations)
: fib ( n -- f ) 0 1 rot 0 do tuck+ loop drop ;
: main ( -- ) 1000000000 fib . cr ;

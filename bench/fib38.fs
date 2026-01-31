\ BENCH compile=10 run=200
\ fib(38) recursive - measures call overhead and recursion
: fib ( n -- f ) dup 2 < if else dup 1- recurse swap 2 - recurse + then ;
: main 38 fib . cr ;

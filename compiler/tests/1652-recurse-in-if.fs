\ expect: 55
: fib ( n -- f ) dup 2 < if else dup 1- recurse swap 2 - recurse + then ;
: main 10 fib . cr ;

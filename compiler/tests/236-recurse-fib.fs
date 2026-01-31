\ expect: 55
: fib dup 2 < if else dup 1- fib swap 2 - fib + then ;
: main 10 fib . cr ;

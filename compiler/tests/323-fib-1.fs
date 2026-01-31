\ expect: 1
\ Test 323: fibonacci of 1
: fib dup 2 < if else dup 1- fib swap 2 - fib + then ;
: main 1 fib . cr ;

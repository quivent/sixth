\ Test 324: fibonacci of 0
: fib dup 2 < if else dup 1- fib swap 2 - fib + then ;
: main 0 fib . cr ;

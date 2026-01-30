\ Test 365: recursive fibonacci of 15
: fib dup 2 < if else dup 1- fib swap 2 - fib + then ;
: main 15 fib . cr ;

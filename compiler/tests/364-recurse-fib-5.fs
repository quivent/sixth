\ Test 364: recursive fibonacci of 5
: fib dup 2 < if else dup 1- fib swap 2 - fib + then ;
: main 5 fib . cr ;

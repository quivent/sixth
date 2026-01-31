\ Test 991: recursive fibonacci(15) = 610
: fib dup 2 < if else dup 1- fib swap 2 - fib + then ;
: main 15 fib . cr ;

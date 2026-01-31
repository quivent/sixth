\ expect: 55
\ Test 322: fibonacci of 10
: fib dup 2 < if else dup 1- fib swap 2 - fib + then ;
: main 10 fib . cr ;

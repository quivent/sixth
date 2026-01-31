\ expect: 610
\ Test 503: fibonacci 15 via recursion
: fib dup 2 < if else dup 1- fib swap 2 - fib + then ;
: main 15 fib . cr ;

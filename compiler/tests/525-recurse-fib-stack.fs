\ expect: 21 0
\ Test 525: fib with extra value on stack (should survive recursion)
: fib dup 2 < if else dup 1- fib swap 2 - fib + then ;
: main 999 8 fib . drop . cr ;

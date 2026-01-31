\ expect: 6765
\ Test 499: fibonacci(20) via recursion
\ Expected output: 6765
: fib dup 2 < if else dup 1- fib swap 2 - fib + then ;
: main 20 fib . cr ;

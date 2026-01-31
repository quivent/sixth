\ expect: 832040
\ Test 974: iterative fibonacci(30) = 832040
: fib 0 1 rot 0 do over + swap loop drop ;
: main 30 fib . cr ;

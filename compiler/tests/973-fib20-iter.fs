\ Test 973: iterative fibonacci(20) = 6765
: fib 0 1 rot 0 do over + swap loop drop ;
: main 20 fib . cr ;

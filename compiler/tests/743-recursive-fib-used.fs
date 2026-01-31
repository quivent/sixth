\ expect: 55
\ Test 743: recursive fibonacci result used
: fib dup 2 < if exit then dup 1- recurse swap 2- recurse + ;
: main 10 fib . cr ;

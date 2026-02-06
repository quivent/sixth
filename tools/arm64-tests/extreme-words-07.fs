\ expect: 89
\ Extreme Test 07: Fibonacci via mutual recursion pair
\ Tests: complex mutual recursion, multiple recursive calls per word

: fib-a ( n -- fib[n] )
  dup 2 < if exit then
  dup 1 - fib-b swap 2 - fib-b + ;

: fib-b ( n -- fib[n] )
  dup 2 < if exit then
  dup 1 - fib-a swap 2 - fib-a + ;

: main 11 fib-a ;

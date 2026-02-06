\ Fibonacci(35) benchmark - recursive
: fib ( n -- n )
  dup 2 < if exit then
  dup 1- recurse swap 2 - recurse + ;

: main
  35 fib . cr ;

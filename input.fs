\ benchmark.fs - Fifth native compiler benchmark suite
\ Run with: time ./output

\ Loop countdown - tests tight loop performance
: bench-loop ( -- n )
  100000000 begin 1-nzloop ;

\ Fibonacci - iterative O(n)
: fib ( n -- fib )
  0 1 rot 0 do tuck+ loop drop ;

\ Nested loops - tests do/loop
: bench-nested ( -- n )
  0 1000 0 do
    1000 0 do
      1+
    loop
  loop ;

\ Arithmetic - tests basic ops
: bench-arith ( -- n )
  0 10000000
  begin swap 1+ swap 1-nzloop
  drop ;

\ Stack manipulation - swap only (rot has depth bug)
: bench-stack ( -- n )
  1 2
  10000000 0 do
    swap
  loop
  drop ;

\ Run all and print results
: main
  bench-loop . cr
  35 fib . cr
  bench-nested . cr
  bench-arith . cr
  bench-stack . cr
;

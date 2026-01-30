\ benchmark.fs - Fifth native compiler benchmark suite
\ Run with: time ./output

\ Loop countdown - tests tight loop performance
: bench-loop ( -- )
  100000000 begin 1-nzloop drop ;

\ Fibonacci - tests recursion (optimized with <if)
: fib ( n -- n )
  dup 2 <if exit then
  dup 1- recurse swap 2- recurse + ;

: bench-fib ( -- )
  35 fib . cr ;

\ Nested loops - tests do/loop
: bench-nested ( -- )
  1000 0 do
    1000 0 do
    loop
  loop ;

\ Arithmetic - tests basic ops
: bench-arith ( -- )
  0
  10000000 0 do
    1+ 1+ 1-
  loop
  drop ;

\ Stack manipulation
: bench-stack ( -- )
  1 2 3
  10000000 0 do
    rot rot rot
  loop
  drop drop drop ;

\ Run all and print marker
: main
  bench-loop
  bench-fib
  bench-nested
  bench-arith
  bench-stack
  0 . cr
;

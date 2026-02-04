\ expected: 46368
\ Fibonacci benchmark - iterative fib(24) run 50000 times

: fib ( n -- f )
  dup 2 < if exit then
  0 1 rot 1 do
    tuck +
  loop nip ;

: main ( -- )
  0
  50000 0 do
    drop 24 fib
  loop
  . cr ;

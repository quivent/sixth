\ expected: 50000005000000
\ Median of 3 benchmark - 10M iterations

10000000 constant ITERS

: median3 ( a b c -- median )
  rot 2dup < if swap then    \ ensure a >= b on stack: c a' b' where a'>=b'
  rot 2dup < if swap then    \ ensure c >= b: a' c' b' where c'>=b'
  drop max ;                  \ max of the two smaller ones

: bench ( -- sum )
  0
  ITERS 0 do
    i i 1+ i 2 + median3 +
  loop ;

: main
  bench . cr ;

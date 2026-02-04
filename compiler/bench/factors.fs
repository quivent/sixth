\ expected: 343614
\ Factorize 100K numbers
\ Checksum: sum of number of prime factors for each

: count-factors ( n -- count )
  dup 2 < if drop 0 exit then
  0 swap
  2
  begin over 1 > while
    2dup mod 0= if
      swap over / swap
      rot 1+ -rot
    else
      dup 2 = if 1+ else 2 + then
    then
    2dup dup * < if
      over 1 > if rot 1+ -rot then
      1 swap
    then
  repeat 2drop ;

: bench-factors ( -- sum )
  0
  100000 0 do
    i 1+ count-factors +
  loop ;

: main bench-factors . cr ;

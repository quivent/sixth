\ expected: 315000000
\ Sum of digits, 10M numbers
\ Checksum: sum of all digit sums

: digit-sum ( n -- sum )
  0 swap
  begin dup 0> while
    dup 10 mod rot + swap
    10 /
  repeat drop ;

: bench-crosssum ( -- sum )
  0
  10000000 0 do
    i digit-sum +
  loop ;

: main bench-crosssum . cr ;

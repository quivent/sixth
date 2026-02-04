\ expected: 3619
\ GCD benchmark - compute GCD of various pairs, 500000 times

: gcd ( a b -- gcd )
  begin dup while tuck mod repeat drop ;

: bench-gcd ( -- sum )
  0
  1071 1029 gcd +
  3528 3780 gcd +
  9999 3333 gcd +
  1234567 7654321 gcd +
  48 18 gcd +
  100 35 gcd +
  17 13 gcd + ;

: main ( -- )
  0
  500000 0 do
    drop bench-gcd
  loop
  . cr ;

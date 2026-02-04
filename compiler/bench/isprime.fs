\ expected: 78498
\ Primality test, 1M numbers
\ Checksum: count of primes found

: is-prime ( n -- 0|1 )
  dup 2 < if drop 0 exit then
  dup 2 = if drop 1 exit then
  dup 2 mod 0= if drop 0 exit then
  dup 3 = if drop 1 exit then
  dup 3 mod 0= if drop 0 exit then
  5
  begin
    2dup dup * >= while
    2dup mod 0= if 2drop 0 exit then
    2 +
    2dup mod 0= if 2drop 0 exit then
    4 +
  repeat 2drop 1 ;

: bench-isprime ( -- count )
  0
  1000000 0 do
    i is-prime +
  loop ;

: main bench-isprime . cr ;

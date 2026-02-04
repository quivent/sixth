\ expected: 1229
\ Primality by trial division - count primes to 10000, run 100 times

: prime? ( n -- flag )
  dup 2 < if drop 0 exit then
  dup 2 = if drop -1 exit then
  dup 1 and 0= if drop 0 exit then
  3
  begin
    2dup dup * >= while
    2dup mod 0= if 2drop 0 exit then
    2 +
  repeat
  2drop -1 ;

: count-primes ( limit -- count )
  0 swap 2 do
    i prime? if 1+ then
  loop ;

: main ( -- )
  0
  100 0 do
    drop 10000 count-primes
  loop
  . cr ;

\ expected: 496267742
\ Find next prime, 100K times
\ Checksum: sum of all next primes (mod 10000)

: is-prime ( n -- 0|1 )
  dup 2 < if drop 0 exit then
  dup 2 = if drop 1 exit then
  dup 2 mod 0= if drop 0 exit then
  3
  begin
    2dup dup * >= while
    2dup mod 0= if 2drop 0 exit then
    2 +
  repeat 2drop 1 ;

: next-prime ( n -- p )
  1+ dup 2 mod 0= if 1+ then
  begin dup is-prime 0= while 2 + repeat ;

: bench-nextprime ( -- sum )
  0
  100000 0 do
    i 100 * next-prime 10000 mod +
  loop ;

: main bench-nextprime . cr ;

\ expected: 4
\ Perfect numbers benchmark - count perfect numbers up to 10000, 100 times

: sum-divisors ( n -- sum )
  1 swap
  dup 2/ 1+ 2 ?do
    dup i mod 0= if i under+ then
  loop drop ;

: perfect? ( n -- flag )
  dup sum-divisors = ;

: count-perfect ( limit -- count )
  0 swap 2 do
    i perfect? if 1+ then
  loop ;

: main ( -- )
  0
  100 0 do
    drop 10000 count-perfect
  loop
  . cr ;

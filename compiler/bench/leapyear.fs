\ expected: 485
\ Leap year benchmark - count leap years 1-2000, run 1000 times

: leap-year? ( y -- flag )
  dup 400 mod 0= if drop -1 exit then
  dup 100 mod 0= if drop 0 exit then
  4 mod 0= ;

: count-leap ( limit -- count )
  0 swap 1+ 1 do
    i leap-year? if 1+ then
  loop ;

: main ( -- )
  0
  1000 0 do
    drop 2000 count-leap
  loop
  . cr ;

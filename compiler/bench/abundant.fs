\ expected: 4953
\ Abundant numbers benchmark - count abundant 1-20000, run 5 times

: proper-divisor-sum ( n -- sum )
  dup 1 = if drop 0 exit then
  1 swap
  dup 2/ 2 do
    dup i mod 0= if i under+ then
  loop drop ;

: abundant? ( n -- flag )
  dup proper-divisor-sum > ;

: count-abundant ( limit -- count )
  0 swap 1+ 1 do
    i abundant? if 1+ then
  loop ;

: main ( -- )
  0
  5 0 do
    drop 20000 count-abundant
  loop
  . cr ;

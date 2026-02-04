\ expected: 262
\ Hailstone/Collatz benchmark - find longest sequence under 10000, run 100 times

: hailstone-len ( n -- len )
  1 swap
  begin dup 1 > while
    dup 1 and if 3 * 1+ else 2/ then
    swap 1+ swap
  repeat drop ;

: max-hailstone ( limit -- max-len )
  0 swap 1 do
    i hailstone-len max
  loop ;

: main ( -- )
  0
  100 0 do
    drop 10000 max-hailstone
  loop
  . cr ;

\ expected: 333833500
\ Sum of squares benchmark - sum of squares 1-1000, run 10000 times

: sum-squares ( n -- sum )
  0 swap 1+ 1 do
    i i * +
  loop ;

: main ( -- )
  0
  10000 0 do
    drop 1000 sum-squares
  loop
  . cr ;

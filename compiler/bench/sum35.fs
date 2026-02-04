\ expected: 233168
\ Sum multiples of 3 and 5 benchmark - sum below 1000, run 100000 times

: sum-multiples ( limit -- sum )
  0 swap 1 do
    i 3 mod 0= if i + else
    i 5 mod 0= if i + then then
  loop ;

: main ( -- )
  0
  100000 0 do
    drop 1000 sum-multiples
  loop
  . cr ;

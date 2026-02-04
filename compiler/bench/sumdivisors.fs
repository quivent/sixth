\ expected: 82256014
\ Sum of divisors benchmark - sum divisor sums 1-10000, run 10 times

: divisor-sum ( n -- sum )
  dup 1 = if drop 1 exit then
  1 swap
  dup 2/ 2 do
    dup i mod 0= if i under+ then
  loop + ;

: sum-all-divisors ( limit -- sum )
  0 swap 1+ 1 do
    i divisor-sum +
  loop ;

: main ( -- )
  0
  10 0 do
    drop 10000 sum-all-divisors
  loop
  . cr ;

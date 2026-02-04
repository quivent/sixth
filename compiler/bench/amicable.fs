\ expected: 31626
\ Amicable pairs benchmark - sum of amicable pairs below 10000, run 5 times

: proper-divisor-sum ( n -- sum )
  dup 1 = if drop 0 exit then
  1 swap
  dup 2/ 2 do
    dup i mod 0= if i under+ then
  loop drop ;

: amicable-sum ( limit -- sum )
  0 swap 2 do
    i proper-divisor-sum
    dup i > if
      dup proper-divisor-sum i = if
        dup i + under+
      else drop then
    else drop then
  loop ;

: main ( -- )
  0
  5 0 do
    drop 10000 amicable-sum
  loop
  . cr ;

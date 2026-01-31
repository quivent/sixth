\ expect: 28
\ Check if 28 is perfect: sum of proper divisors = 28
\ Divisors of 28: 1 2 4 7 14 => sum=28
: divisor-sum ( n -- sum )
  0 over 2 / 1+ 1 do
    over i mod 0= if i + then
  loop swap drop ;
: main
  28 dup divisor-sum
  over = if . else drop 0 . then cr ;

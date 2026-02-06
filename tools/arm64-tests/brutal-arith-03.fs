\ expect: 0
\ Test: Division with negative numbers (symmetric division)
\ Standard Forth uses symmetric division (rounds toward zero)

: test1
  \ Positive / Positive = Positive
  7 3 / 2 = 0= if 1 exit then
  7 3 mod 1 = 0= if 2 exit then
  0 ;

: test2
  \ Negative / Positive: -7/3 should be -2 (symmetric) not -3 (floored)
  -7 3 / -2 = 0= if 3 exit then
  \ Remainder: -7 = 3*(-2) + (-1), so mod = -1
  -7 3 mod -1 = 0= if 4 exit then
  0 ;

: test3
  \ Positive / Negative: 7/-3 should be -2 (symmetric)
  7 -3 / -2 = 0= if 5 exit then
  \ Remainder: 7 = -3*(-2) + 1, so mod = 1
  7 -3 mod 1 = 0= if 6 exit then
  0 ;

: test4
  \ Negative / Negative: -7/-3 should be 2
  -7 -3 / 2 = 0= if 7 exit then
  \ Remainder: -7 = -3*2 + (-1), so mod = -1
  -7 -3 mod -1 = 0= if 8 exit then
  0 ;

: main
  test1 dup 0<> if exit then drop
  test2 dup 0<> if exit then drop
  test3 dup 0<> if exit then drop
  test4 ;

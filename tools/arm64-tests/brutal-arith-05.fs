\ expect: 0
\ Test: NEGATE and ABS edge cases, especially MIN-INT

: max-int -1 1 rshift ;
: min-int 1 63 lshift ;

: test1
  \ Basic NEGATE
  5 negate -5 = 0= if 1 exit then
  -5 negate 5 = 0= if 2 exit then
  0 negate 0= 0= if 3 exit then
  0 ;

: test2
  \ NEGATE of MIN-INT is MIN-INT (cannot represent positive equivalent)
  min-int negate min-int = 0= if 4 exit then
  0 ;

: test3
  \ NEGATE of MAX-INT should work: -(MAX-INT) = MIN-INT + 1
  \ i.e., max-int negate = min-int + 1
  max-int negate min-int 1 + = 0= if 5 exit then
  0 ;

: test4
  \ Basic ABS
  5 abs 5 = 0= if 6 exit then
  -5 abs 5 = 0= if 7 exit then
  0 abs 0= 0= if 8 exit then
  0 ;

: test5
  \ ABS of MIN-INT is still MIN-INT (edge case!)
  \ Because the positive value cannot be represented
  min-int abs min-int = 0= if 9 exit then
  0 ;

: test6
  \ ABS of MAX-INT is MAX-INT
  max-int abs max-int = 0= if 10 exit then
  0 ;

: main
  test1 dup 0<> if exit then drop
  test2 dup 0<> if exit then drop
  test3 dup 0<> if exit then drop
  test4 dup 0<> if exit then drop
  test5 dup 0<> if exit then drop
  test6 ;

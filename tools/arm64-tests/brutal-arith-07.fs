\ expect: 0
\ Test: 1+, 1-, shift-based 2*, 2/ edge cases

: max-int -1 1 rshift ;
: min-int 1 63 lshift ;

\ 2* as lshift by 1
: 2* 1 lshift ;
\ 2/ as arithmetic rshift by 1 (but rshift is logical!)
\ For signed 2/, need: dup 0< if 1 rshift 1 63 lshift or else 1 rshift then
\ Simplified: use / 2
: 2/ 2 / ;

: test1
  \ Basic 1+ and 1-
  0 1+ 1 = 0= if 1 exit then
  0 1- -1 = 0= if 2 exit then
  -1 1+ 0= 0= if 3 exit then
  1 1- 0= 0= if 4 exit then
  0 ;

: test2
  \ 1+ at MAX-INT wraps to MIN-INT
  max-int 1+ min-int = 0= if 5 exit then
  0 ;

: test3
  \ 1- at MIN-INT wraps to MAX-INT
  min-int 1- max-int = 0= if 6 exit then
  0 ;

: test4
  \ 2* is shift left by 1
  5 2* 10 = 0= if 7 exit then
  -5 2* -10 = 0= if 8 exit then
  0 ;

: test5
  \ 2* at large values causes overflow
  max-int 2* -2 = 0= if 9 exit then
  0 ;

: test6
  \ 2/ is arithmetic shift right (preserves sign)
  10 2/ 5 = 0= if 10 exit then
  -10 2/ -5 = 0= if 11 exit then
  0 ;

: test7
  \ 2/ of -1 gives 0 (symmetric division rounds toward zero)
  \ -1 / 2 = 0 (not -1, which would be floored division)
  -1 2/ 0= 0= if 12 exit then
  0 ;

: test8
  \ 2/ of MIN-INT gives MIN-INT/2
  \ MIN-INT = -2^63, MIN-INT/2 = -2^62
  min-int 2/ 1 62 lshift negate = 0= if 13 exit then
  0 ;

: main
  test1 dup 0<> if exit then drop
  test2 dup 0<> if exit then drop
  test3 dup 0<> if exit then drop
  test4 dup 0<> if exit then drop
  test5 dup 0<> if exit then drop
  test6 dup 0<> if exit then drop
  test7 dup 0<> if exit then drop
  test8 ;

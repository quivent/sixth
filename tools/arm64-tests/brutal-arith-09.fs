\ expect: 0
\ Test: Chained operations that could expose register issues

: max-int -1 1 rshift ;
: min-int 1 63 lshift ;

: test1
  \ Chain of additions
  1 2 + 3 + 4 + 5 + 15 = 0= if 1 exit then
  0 ;

: test2
  \ Chain of subtractions (left associative)
  100 10 - 5 - 3 - 82 = 0= if 2 exit then
  0 ;

: test3
  \ Mixed add/sub chain
  100 50 - 25 + 10 - 5 + 70 = 0= if 3 exit then
  0 ;

: test4
  \ Alternating negate (even = original)
  5 negate negate negate negate 5 = 0= if 4 exit then
  0 ;

: test5
  \ Alternating negate (odd = negated)
  5 negate negate negate -5 = 0= if 5 exit then
  0 ;

: test6
  \ Chain of 1+ operations
  0 1+ 1+ 1+ 1+ 1+ 1+ 1+ 1+ 1+ 1+ 10 = 0= if 6 exit then
  0 ;

: test7
  \ Chain of shift operations (2* via lshift)
  1 1 lshift 1 lshift 1 lshift 1 lshift 16 = 0= if 7 exit then
  0 ;

: test8
  \ Chain of rshift operations
  256 1 rshift 1 rshift 1 rshift 1 rshift 16 = 0= if 8 exit then
  0 ;

: test9
  \ Complex expression: (5 + 3) * 2 - 4 = 12
  5 3 + 2 * 4 - 12 = 0= if 9 exit then
  0 ;

: test10
  \ Deeply nested: ((10 - 3) * 2 + 6) / 2 = 10
  10 3 - 2 * 6 + 2 / 10 = 0= if 10 exit then
  0 ;

: main
  test1 dup 0<> if exit then drop
  test2 dup 0<> if exit then drop
  test3 dup 0<> if exit then drop
  test4 dup 0<> if exit then drop
  test5 dup 0<> if exit then drop
  test6 dup 0<> if exit then drop
  test7 dup 0<> if exit then drop
  test8 dup 0<> if exit then drop
  test9 dup 0<> if exit then drop
  test10 ;

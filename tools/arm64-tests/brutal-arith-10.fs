\ expect: 0
\ Test: Mixed operations that could confuse optimization passes

: max-int -1 1 rshift ;
: min-int 1 63 lshift ;

: test1
  \ Identity operations that optimizers might mishandle
  42 0 + 42 = 0= if 1 exit then
  42 0 - 42 = 0= if 2 exit then
  42 1 * 42 = 0= if 3 exit then
  42 1 / 42 = 0= if 4 exit then
  42 1 mod 0= 0= if 5 exit then
  0 ;

: test2
  \ Self-canceling operations
  17 5 + 5 - 17 = 0= if 6 exit then
  17 5 - 5 + 17 = 0= if 7 exit then
  0 ;

: test3
  \ Multiply then divide should give back (for exact division)
  17 3 * 3 / 17 = 0= if 8 exit then
  0 ;

: test4
  \ ABS and NEGATE combinations
  -42 abs negate -42 = 0= if 9 exit then
  42 negate abs 42 = 0= if 10 exit then
  0 ;

: test5
  \ Same value comparisons
  \ 42 42 = gives -1 (true)
  42 42 = 0= if 11 exit then        \ should NOT take branch (= gives -1, not 0)
  42 42 < if 12 exit then           \ should NOT take branch
  42 42 > if 13 exit then           \ should NOT take branch
  0 ;

: test6
  \ Division where result is exact
  100 5 / 20 = 0= if 14 exit then
  100 5 mod 0= 0= if 15 exit then
  0 ;

: test7
  \ Power of 2 multiplication/division equivalence
  7 8 * 7 1 lshift 1 lshift 1 lshift = 0= if 16 exit then
  0 ;

: test8
  \ Large number arithmetic
  1000000000 1000000000 + 2000000000 = 0= if 17 exit then
  1000000000 1000 * 1000000000000 = 0= if 18 exit then
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

\ expect: 0
\ Test: Division invariant - n1 = n2 * quot + rem
\ Verify: n1 n2 /  n1 n2 mod  together satisfy this

: check-divmod ( n1 n2 -- flag )
  2dup / >r       \ save quotient
  2dup mod        \ get remainder
  swap drop       \ n1 rem
  swap r>         \ rem n1 quot
  rot             \ n1 quot rem
  -rot            \ rem n1 quot
  >r swap r>      \ n1 rem quot
  \ Now we have: n1 rem quot
  \ Want: n2 * quot + rem = n1
  \ But we lost n2. Restructure:
  ;

\ Simpler approach: verify the invariant directly
: test1
  \ 17 = 5 * 3 + 2
  17 5 / 3 = 0= if 1 exit then
  17 5 mod 2 = 0= if 2 exit then
  5 3 * 2 + 17 = 0= if 3 exit then
  0 ;

: test2
  \ -17 = 5 * (-3) + (-2)  [symmetric: round toward zero]
  -17 5 / -3 = 0= if 4 exit then
  -17 5 mod -2 = 0= if 5 exit then
  5 -3 * -2 + -17 = 0= if 6 exit then
  0 ;

: test3
  \ 17 = -5 * (-3) + 2
  17 -5 / -3 = 0= if 7 exit then
  17 -5 mod 2 = 0= if 8 exit then
  -5 -3 * 2 + 17 = 0= if 9 exit then
  0 ;

: test4
  \ -17 = -5 * 3 + (-2)
  -17 -5 / 3 = 0= if 10 exit then
  -17 -5 mod -2 = 0= if 11 exit then
  -5 3 * -2 + -17 = 0= if 12 exit then
  0 ;

: test5
  \ Dividend is 0
  0 5 / 0= 0= if 13 exit then
  0 5 mod 0= 0= if 14 exit then
  0 -5 / 0= 0= if 15 exit then
  0 -5 mod 0= 0= if 16 exit then
  0 ;

: main
  test1 dup 0<> if exit then drop
  test2 dup 0<> if exit then drop
  test3 dup 0<> if exit then drop
  test4 dup 0<> if exit then drop
  test5 ;

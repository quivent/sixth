\ Adversarial Stack Test 14: Nested call stack preservation
\ expect: 0
\ Test deep call chains preserve stack correctly

: level4 ( n -- n ) 1+ ;
: level3 ( n -- n ) level4 1+ ;
: level2 ( n -- n ) level3 1+ ;
: level1 ( n -- n ) level2 1+ ;

: t-deep-call ( -- flag )
  0 level1
  \ 0 -> 1 -> 2 -> 3 -> 4
  4 = if 0 else 1 then ;

: pass-thru ( a b c -- a b c ) ;

: t-passthru ( -- flag )
  1 2 3 pass-thru
  3 = swap 2 = and swap 1 = and
  if 0 else 1 then ;

: swap-help ( a b -- b a ) swap ;

: t-nest-swap ( -- flag )
  10 20 swap-help swap-help swap-help
  \ 3 swaps = 1 net swap
  10 = swap 20 = and
  if 0 else 1 then ;

: sum3 ( a b c -- sum ) + + ;

: t-cons-all ( -- flag )
  5 10 15 sum3
  30 = if 0 else 1 then ;

: mul-add ( a b c -- a*b+c )
  rot rot * + ;

: t-cmplx-call ( -- flag )
  2 3 4 mul-add
  \ 2*3+4 = 10
  10 = if 0 else 1 then ;

: rec-sum ( n -- sum )
  dup 0= if
    drop 0
  else
    dup 1- rec-sum +
  then ;

: t-recursive ( -- flag )
  5 rec-sum
  \ 5+4+3+2+1+0 = 15
  15 = if 0 else 1 then ;

: main
  t-deep-call
  t-passthru +
  t-nest-swap +
  t-cons-all +
  t-cmplx-call +
  t-recursive + ;

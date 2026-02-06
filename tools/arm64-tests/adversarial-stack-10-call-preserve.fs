\ Adversarial Stack Test 10: Stack preservation across calls
\ expect: 0
\ Test that stack state survives function calls

: helper1 ( -- n ) 42 ;
: helper2 ( n -- n ) 1+ ;
: helper3 ( a b -- c ) + ;

: t-call-simp ( -- flag )
  helper1
  42 = if 0 else 1 then ;

: t-call-stk ( -- flag )
  10 20
  helper1
  \ Stack: 10 20 42
  42 = swap 20 = and swap 10 = and
  if 0 else 1 then ;

: t-call-cons ( -- flag )
  100 helper2 helper2 helper2
  \ 100 -> 101 -> 102 -> 103
  103 = if 0 else 1 then ;

: t-call-bin ( -- flag )
  15 25 helper3
  \ 15 + 25 = 40
  40 = if 0 else 1 then ;

: t-call-chn ( -- flag )
  5 10
  helper3      \ 15
  helper1      \ 15 42
  helper3      \ 57
  57 = if 0 else 1 then ;

: double ( n -- n*2 ) dup + ;

: t-nested ( -- flag )
  3 double double double
  \ 3 -> 6 -> 12 -> 24
  24 = if 0 else 1 then ;

: main
  t-call-simp
  t-call-stk +
  t-call-cons +
  t-call-bin +
  t-call-chn +
  t-nested + ;

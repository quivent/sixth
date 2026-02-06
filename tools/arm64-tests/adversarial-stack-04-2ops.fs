\ Adversarial Stack Test 04: 2dup, 2drop, -rot with edge values
\ expect: 0
\ Test double-word operations

: t-2dup ( -- flag )
  100 200 2dup
  \ Stack: 100 200 100 200
  200 = swap 100 = and swap 200 = and swap 100 = and
  if 0 else 1 then ;

: t-2drop ( -- flag )
  1 2 3 4 2drop
  \ Stack: 1 2
  2 = swap 1 = and
  if 0 else 1 then ;

: t-2dup-ext ( -- flag )
  0 -1 2dup
  \ Stack: 0 -1 0 -1
  -1 = swap 0= and swap -1 = and swap 0= and
  if 0 else 1 then ;

: t-minusrot ( -- flag )
  1 2 3 -rot
  \ 1 2 3 -> 3 1 2
  2 = swap 1 = and swap 3 = and
  if 0 else 1 then ;

: t-chain ( -- flag )
  10 20 2dup 2drop
  \ Stack: 10 20
  20 = swap 10 = and
  if 0 else 1 then ;

: main
  t-2dup
  t-2drop +
  t-2dup-ext +
  t-minusrot +
  t-chain + ;

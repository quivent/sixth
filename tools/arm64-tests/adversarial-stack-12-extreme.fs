\ Adversarial Stack Test 12: Extreme values
\ expect: 0
\ Test stack ops with boundary values

: t-zero ( -- flag )
  0 dup +
  0= if 0 else 1 then ;

: t-neg-one ( -- flag )
  -1 dup
  -1 = swap -1 = and
  if 0 else 1 then ;

: t-large ( -- flag )
  1000000000 dup swap
  1000000000 = swap 1000000000 = and
  if 0 else 1 then ;

: t-negative ( -- flag )
  -999999 dup over
  \ Stack: -999999 -999999 -999999
  -999999 = swap -999999 = and swap -999999 = and
  if 0 else 1 then ;

: t-mix-sign ( -- flag )
  -100 100 swap
  \ -100 100 -> 100 -100
  -100 = swap 100 = and
  if 0 else 1 then ;

: t-bound-rot ( -- flag )
  -1 0 1 rot
  \ -1 0 1 -> 0 1 -1
  -1 = swap 1 = and swap 0= and
  if 0 else 1 then ;

: main
  t-zero
  t-neg-one +
  t-large +
  t-negative +
  t-mix-sign +
  t-bound-rot + ;

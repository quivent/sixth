\ Adversarial Stack Test 08: Stack balance across conditionals
\ expect: 0
\ Test that both branches maintain stack balance

: t-if-then ( -- flag )
  1 2 3
  1 if
    swap
  then
  \ 1 2 3 -> 1 3 2 (when true)
  2 = swap 3 = and swap 1 = and
  if 0 else 1 then ;

: t-if-else ( -- flag )
  10 20
  0 if
    swap
  else
    dup
  then
  \ 10 20 -> 10 20 20 (false path)
  20 = swap 20 = and swap 10 = and
  if 0 else 1 then ;

: t-nested-if ( -- flag )
  100
  1 if
    1 if
      1+
    then
    1+
  then
  \ 100 -> 101 -> 102
  102 = if 0 else 1 then ;

: t-if-deep ( -- flag )
  1 2 3 4 5
  1 if
    rot       \ 1 2 4 5 3
    swap      \ 1 2 4 3 5
  then
  5 = swap 3 = and swap 4 = and swap 2 = and swap 1 = and
  if 0 else 1 then ;

: main
  t-if-then
  t-if-else +
  t-nested-if +
  t-if-deep + ;

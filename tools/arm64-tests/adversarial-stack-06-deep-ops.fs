\ Adversarial Stack Test 06: Operations at various depths
\ expect: 0
\ Test stack manipulation at different depths

: t-deep-over ( -- flag )
  1 2 3 4 5 6 7 8 9 10
  over
  \ Stack now: 1 2 3 4 5 6 7 8 9 10 9 (11 items)
  9 = if
    \ Correct - clean up
    drop drop drop drop drop
    drop drop drop drop drop
    0
  else
    drop drop drop drop drop
    drop drop drop drop drop
    1
  then ;

: t-deep-swap ( -- flag )
  1 2 3 4 5 6 7 8 9 10
  swap
  \ Stack now: 1 2 3 4 5 6 7 8 10 9 (10 items)
  9 = if
    drop drop drop drop drop
    drop drop drop drop
    0
  else
    drop drop drop drop drop
    drop drop drop drop
    1
  then ;

: t-deep-rot ( -- flag )
  1 2 3 4 5 6 7 8 9 10
  rot
  \ Stack: 1 2 3 4 5 6 7 9 10 8 (10 items)
  8 = if
    drop drop drop drop drop
    drop drop drop drop
    0
  else
    drop drop drop drop drop
    drop drop drop drop
    1
  then ;

: t-deep-dup ( -- flag )
  1 2 3 4 5 6 7 8 9 10
  dup
  \ Stack: 1 2 3 4 5 6 7 8 9 10 10 (11 items)
  10 = if
    drop drop drop drop drop
    drop drop drop drop drop
    0
  else
    drop drop drop drop drop
    drop drop drop drop drop
    1
  then ;

: main
  t-deep-over
  t-deep-swap +
  t-deep-rot +
  t-deep-dup + ;

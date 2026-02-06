\ Adversarial Stack Test 03: Complex swap/rot/over sequences
\ expect: 0
\ Test multiple rotations and swaps in sequence

: t-swap-chain ( -- flag )
  1 2 swap swap swap swap
  \ Should be: 1 2
  2 = swap 1 = and
  if 0 else 1 then ;

: t-rot-chain ( -- flag )
  1 2 3 rot rot rot
  \ 1 2 3 -> 2 3 1 -> 3 1 2 -> 1 2 3
  3 = -rot 2 = swap 1 = and and
  if 0 else 1 then ;

: t-mixed ( -- flag )
  1 2 3 4 5
  swap      \ 1 2 3 5 4
  rot       \ 1 2 5 4 3
  over      \ 1 2 5 4 3 4
  -rot      \ 1 2 5 3 4 4
  drop drop drop drop drop drop
  0 ;

: t-over-rot ( -- flag )
  10 20 30
  over      \ 10 20 30 20
  rot       \ 10 30 20 20
  20 = swap 20 = and swap 30 = and swap 10 = and
  if 0 else 1 then ;

: main
  t-swap-chain
  t-rot-chain +
  t-mixed +
  t-over-rot + ;

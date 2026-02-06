\ Adversarial Stack Test 05: nip and tuck sequences
\ expect: 0
\ Test nip and tuck in various combinations

: t-nip ( -- flag )
  1 2 nip
  \ ( 1 2 -- 2 )
  2 = if 0 else 1 then ;

: t-tuck ( -- flag )
  1 2 tuck
  \ ( 1 2 -- 2 1 2 )
  2 = swap 1 = and swap 2 = and
  if 0 else 1 then ;

: t-nip-chain ( -- flag )
  1 2 3 4 5 nip nip nip nip
  \ 1 2 3 4 5 -> 1 2 3 5 -> 1 2 5 -> 1 5 -> 5
  5 = if 0 else 1 then ;

: t-tuck-nip ( -- flag )
  10 20 tuck nip
  \ 10 20 -> 20 10 20 -> 20 20
  20 = swap 20 = and
  if 0 else 1 then ;

: t-nip-tuck ( -- flag )
  1 2 3
  nip       \ 1 3
  4 tuck    \ 1 4 3 4
  drop drop drop drop
  0 ;

: main
  t-nip
  t-tuck +
  t-nip-chain +
  t-tuck-nip +
  t-nip-tuck + ;

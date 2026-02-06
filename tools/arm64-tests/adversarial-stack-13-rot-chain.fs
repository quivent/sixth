\ Adversarial Stack Test 13: Rotation chains
\ expect: 0
\ Test extensive rotation sequences

: t-rot-id ( -- flag )
  1 2 3
  rot rot rot
  \ 3 rotations on 3 items = identity
  3 = swap 2 = and swap 1 = and
  if 0 else 1 then ;

: t-mrot-id ( -- flag )
  1 2 3
  -rot -rot -rot
  \ 3 -rots = identity
  3 = swap 2 = and swap 1 = and
  if 0 else 1 then ;

: t-rot-mrot ( -- flag )
  1 2 3
  rot -rot
  \ rot then -rot = identity
  3 = swap 2 = and swap 1 = and
  if 0 else 1 then ;

: t-dbl-rot ( -- flag )
  1 2 3
  rot rot
  \ 1 2 3 -> 2 3 1 -> 3 1 2
  2 = swap 1 = and swap 3 = and
  if 0 else 1 then ;

: t-alt ( -- flag )
  10 20 30
  rot -rot rot -rot rot
  \ Odd number of pairs + 1 rot = 1 net rot
  \ 10 20 30 -> 20 30 10
  10 = swap 30 = and swap 20 = and
  if 0 else 1 then ;

: t-long-chn ( -- flag )
  1 2 3
  rot rot rot rot rot rot
  \ 6 rots = 2 complete cycles = identity
  3 = swap 2 = and swap 1 = and
  if 0 else 1 then ;

: main
  t-rot-id
  t-mrot-id +
  t-rot-mrot +
  t-dbl-rot +
  t-alt +
  t-long-chn + ;

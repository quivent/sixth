\ expect: 0
\ Test: Triple ROT - should cycle back to original
\ rot rot rot = identity

: main
  1000 2000 3000
  rot rot rot   ( 3 rotations = identity )
  3000 - swap 2000 - or swap 1000 - or
;

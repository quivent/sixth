\ expect: 0
\ Test: 2DUP on double values - must copy both cells correctly
\ 2DUP: (a b -- a b a b)

: main
  1111111111 2222222222
  2dup              ( Stack: 1111... 2222... 1111... 2222... )
  2222222222 - swap 1111111111 - or
  ( check copied pair )
  -rot
  2222222222 - swap 1111111111 - or
  ( check original pair )
  or
;

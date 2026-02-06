\ Adversarial arithmetic test: negate MIN_INT64
\ expect: 0
\ negate MIN_INT64 = MIN_INT64 (overflow, -MIN can't be represented)
: main
  1 63 lshift negate  \ Should still be MIN_INT64
  1 63 lshift = if 0 else 1 then ;

\ Adversarial arithmetic test: Shift by zero (should be identity)
\ expect: 0
: main
  42 0 lshift 42 = if
  42 0 rshift 42 = if
  0
  else 2 then
  else 1 then ;

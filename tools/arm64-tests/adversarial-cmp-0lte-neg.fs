\ Adversarial: 0<= with -1 should be true (-1)
\ expect: 1
: main
  -1 0<=           \ -1 <= 0 is TRUE, should return -1
  -1 = if 1 else 0 then ;

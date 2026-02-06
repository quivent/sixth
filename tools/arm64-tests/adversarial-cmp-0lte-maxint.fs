\ Adversarial: 0<= with MAX-INT (most positive) should be false (0)
\ expect: 1
: main
  1 63 lshift 1 -  \ MAX-INT: 9223372036854775807 on 64-bit
  0<=              \ MAX-INT <= 0 is FALSE
  0= if 1 else 0 then ;

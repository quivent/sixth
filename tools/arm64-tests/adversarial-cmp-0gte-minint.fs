\ Adversarial: 0>= with MIN-INT (most negative) should be false (0)
\ expect: 1
: main
  1 63 lshift      \ -9223372036854775808 (MIN-INT on 64-bit)
  0>=              \ MIN-INT >= 0 is FALSE
  0= if 1 else 0 then ;

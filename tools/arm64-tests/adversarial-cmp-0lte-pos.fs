\ Adversarial: 0<= with 1 should be false (0)
\ expect: 1
: main
  1 0<=            \ 1 <= 0 is FALSE, should return 0
  0= if 1 else 0 then ;

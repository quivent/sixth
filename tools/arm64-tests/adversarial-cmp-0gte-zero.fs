\ Adversarial: 0>= with 0 should be true (-1)
\ expect: 1
: main
  0 0>=            \ 0 >= 0 should be -1 (true)
  -1 = if 1 else 0 then ;

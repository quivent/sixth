\ Adversarial: Chained comparisons with 0>= and 0<=
\ expect: 1
: main
  \ Test: 5 >= 0 AND -5 <= 0 AND 0 >= 0 AND 0 <= 0
  5 0>=            \ true (-1)
  -5 0<=           \ true (-1)
  and              \ -1 AND -1 = -1
  0 0>=            \ true (-1)
  and              \ -1 AND -1 = -1
  0 0<=            \ true (-1)
  and              \ -1 AND -1 = -1
  -1 = if 1 else 0 then ;

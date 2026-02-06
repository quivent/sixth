\ Adversarial: Boundary tests - numbers just above/below zero
\ expect: 1
: main
  \ 1 is just above 0
  1 0>=            \ true (-1)
  1 0<=            \ false (0)
  xor              \ -1 XOR 0 = -1

  \ -1 is just below 0
  -1 0>=           \ false (0)
  -1 0<=           \ true (-1)
  xor              \ 0 XOR -1 = -1

  and              \ -1 AND -1 = -1
  -1 = if 1 else 0 then ;

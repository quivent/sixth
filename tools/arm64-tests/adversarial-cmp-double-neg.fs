\ Adversarial: Double negation edge case
\ expect: 1
: main
  \ negate returns the two's complement
  \ negate of 0 is 0
  0 negate 0>=     \ 0 >= 0 is true (-1)
  0 negate 0<=     \ 0 <= 0 is true (-1)
  and              \ -1 AND -1 = -1

  \ negate of 1 is -1
  1 negate 0>=     \ -1 >= 0 is false (0)
  1 negate 0<=     \ -1 <= 0 is true (-1)
  xor              \ 0 XOR -1 = -1

  and              \ -1 AND -1 = -1
  -1 = if 1 else 0 then ;

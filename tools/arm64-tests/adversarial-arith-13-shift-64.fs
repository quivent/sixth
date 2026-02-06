\ Adversarial arithmetic test: Shift by 64 bits (full word width)
\ expect: 0
\ ARM64: shift amount is mod 64, so shift by 64 = shift by 0
\ 42 << 64 = 42, 42 >> 64 = 42
: main
  42 64 lshift 42 = if
  42 64 rshift 42 = if
  0
  else 2 then
  else 1 then ;

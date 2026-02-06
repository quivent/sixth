\ Adversarial arithmetic test: Shift by 63 bits
\ expect: 0
\ 1 << 63 = MIN_INT64 (sign bit)
\ MIN_INT64 >> 63 = 1 (logical shift right)
: main
  1 63 lshift             \ Should be MIN_INT64
  63 rshift               \ Should be 1
  1 = if 0 else 1 then ;

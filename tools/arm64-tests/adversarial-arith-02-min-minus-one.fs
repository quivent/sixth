\ Adversarial arithmetic test: MIN_INT64 - 1 should overflow to MAX_INT64
\ expect: 0
\ MIN_INT64 = -2^63 = 1 << 63 (only sign bit set)
\ We compute MIN_INT64 as: 1 63 lshift
\ Then subtract 1 -> should wrap to MAX_INT64 (positive)
: main
  1 63 lshift       \ MIN_INT64 (sign bit only)
  1 -               \ Should wrap to MAX_INT64
  0>                \ Is it positive? Should be -1
  1 + ;             \ -1 + 1 = 0 (PASS)

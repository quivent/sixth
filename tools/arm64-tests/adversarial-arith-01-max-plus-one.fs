\ Adversarial arithmetic test: MAX_INT64 + 1 should overflow to MIN_INT64
\ expect: 0
\ MAX_INT64 = 2^63 - 1 = 9223372036854775807
\ We compute MAX_INT64 as: -1 1 rshift (all bits except sign)
\ Then add 1 -> should wrap to MIN_INT64 (negative)
: main
  -1 1 rshift       \ MAX_INT64 (logical shift, fills 0 at top)
  1 +               \ Should wrap to MIN_INT64
  0<                \ Is it negative? Should be -1
  1 + ;             \ -1 + 1 = 0 (PASS)

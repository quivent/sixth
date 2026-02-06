\ Adversarial arithmetic test: Multiplication overflow
\ expect: 0
\ Large positive * 2 should overflow
\ 2^62 * 2 = 2^63 = MIN_INT64 (overflow into sign bit)
\ 2^62 = 1 << 62
: main
  1 62 lshift 2 *   \ Should overflow to MIN_INT64
  0<                 \ Is it negative? Should be -1
  1 + ;              \ -1 + 1 = 0 (PASS)

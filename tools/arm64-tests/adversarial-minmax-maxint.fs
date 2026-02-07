\ expect: 0
\ Adversarial: MAX_INT64 (0x7FFFFFFFFFFFFFFF) edge cases
\ MAX_INT is the most positive 64-bit signed value: 9223372036854775807
\ Returns 0 if all tests pass
: main
  0                          \ start accumulator at 0

  \ Test 1: min(MAX_INT, 0) should = 0
  1 63 lshift 1- 0 min       \ compute min
  abs +                      \ should be 0, add to accumulator

  \ Test 2: max(MAX_INT, 0) should = MAX_INT
  1 63 lshift 1- 0 max       \ compute max
  1 63 lshift 1- - abs +     \ compare with MAX_INT

  \ Test 3: min(MAX_INT, -1) should = -1
  1 63 lshift 1- -1 min      \ compute min
  -1 - abs +                 \ compare with -1

  \ Test 4: max(MAX_INT, -1) should = MAX_INT
  1 63 lshift 1- -1 max      \ compute max
  1 63 lshift 1- - abs +     \ compare with MAX_INT
;

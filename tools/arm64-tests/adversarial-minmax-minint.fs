\ expect: 0
\ Adversarial: MIN_INT64 (0x8000000000000000) edge cases
\ MIN_INT is the most negative 64-bit signed value: -9223372036854775808
\ Special: negate(MIN_INT) overflows back to MIN_INT
\ Returns 0 if all tests pass
: main
  1 63 lshift            \ MIN_INT = 0x8000000000000000
  dup dup 0 min - abs    \ min(MIN_INT, 0) should = MIN_INT
  swap                   \ ( sum MIN_INT )
  dup 0 max abs +        \ max(MIN_INT, 0) should = 0
  swap                   \ ( sum MIN_INT )
  dup 1 min swap - abs + \ min(MIN_INT, 1) should = MIN_INT
  1 63 lshift            \ get MIN_INT again
  1 max 1 - abs +        \ max(MIN_INT, 1) should = 1
;

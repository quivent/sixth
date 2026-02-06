\ Adversarial arithmetic test: Chained additions near overflow boundary
\ expect: 0
\ Start at MAX_INT64-2, add 1 three times, verify wrap
: test-chain ( -- flag )
  -1 1 rshift       \ MAX_INT64
  2 -               \ MAX_INT64 - 2
  1 +               \ MAX_INT64 - 1
  1 +               \ MAX_INT64
  1 +               \ MIN_INT64 (wrapped)
  0< ;              \ Should be -1 (negative)

: main
  test-chain 1 + ;  \ -1 + 1 = 0 (PASS)

\ Adversarial arithmetic test: Modulo with negative dividend
\ expect: 0
\ -7 mod 3 = -1 (truncated division: -7 = -2*3 + (-1))
\ Verify: -7 / 3 = -2 (truncated toward zero)
\ Remainder: -7 - (-2)*3 = -7 + 6 = -1
: main
  -7 3 mod     \ Should be -1
  1 + ;        \ -1 + 1 = 0 (PASS)

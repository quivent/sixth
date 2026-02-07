\ expect: 0
\ Pick with extreme 64-bit values on the stack
\ Tests that pick doesn't corrupt large values during copy
: main
  -9223372036854775808   \ minimum signed 64-bit
  9223372036854775807    \ maximum signed 64-bit
  0                      \ zero
  -1                     \ all bits set
  \ Stack: min max 0 -1 (top), indices 0-3
  \ 0 pick = -1, 1 pick = 0, 2 pick = max, 3 pick = min

  0   \ accumulator
  \ Stack now: min max 0 -1 acc (indices 0-4)
  \ Original values shifted by 1

  1 pick -1 - abs +                        \ was 0 pick, should be -1
  2 pick 0 - abs +                         \ was 1 pick, should be 0
  3 pick 9223372036854775807 - abs +       \ was 2 pick, should be max
  4 pick -9223372036854775808 - abs +      \ was 3 pick, should be min

  >r drop drop drop drop r>
;

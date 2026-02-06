\ expect: 0
\ Test: Signed comparison with negative numbers
\ < and > are SIGNED comparisons in standard Forth

: main
  -1 0 < -1 <> if 1 exit then     \ -1 < 0 must be true (signed)
  0 -1 > -1 <> if 2 exit then     \ 0 > -1 must be true (signed)
  -1 1 < -1 <> if 3 exit then     \ -1 < 1 must be true (signed)
  1 -1 > -1 <> if 4 exit then     \ 1 > -1 must be true (signed)

  -5 -3 < -1 <> if 5 exit then    \ -5 < -3 must be true
  -3 -5 > -1 <> if 6 exit then    \ -3 > -5 must be true
  -3 -3 < 0 <> if 7 exit then     \ -3 < -3 must be false
  -3 -3 > 0 <> if 8 exit then     \ -3 > -3 must be false

  0   \ all passed
;

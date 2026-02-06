\ expect: 0
\ Test: <= and >= comparisons (signed)

: main
  \ <= tests
  0 0 <= -1 <> if 1 exit then               \ 0 <= 0
  -1 0 <= -1 <> if 2 exit then              \ -1 <= 0
  0 1 <= -1 <> if 3 exit then               \ 0 <= 1
  1 0 <= 0 <> if 4 exit then                \ NOT (1 <= 0)

  \ >= tests
  0 0 >= -1 <> if 5 exit then               \ 0 >= 0
  0 -1 >= -1 <> if 6 exit then              \ 0 >= -1
  1 0 >= -1 <> if 7 exit then               \ 1 >= 0
  0 1 >= 0 <> if 8 exit then                \ NOT (0 >= 1)

  \ Edge cases with large negative
  -100 -1 <= -1 <> if 9 exit then           \ -100 <= -1
  -1 -100 >= -1 <> if 10 exit then          \ -1 >= -100

  0
;

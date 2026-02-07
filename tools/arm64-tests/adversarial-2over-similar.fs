\ expect: 0
\ adversarial-2over-similar.fs - Test 2over with similar values to catch offset bugs
\ Tests: If x1 x2 could be confused with x3 x4 due to wrong offset
\ Edge case: Adjacent values that differ by small amounts
\
\ 2over: ( x1 x2 x3 x4 -- x1 x2 x3 x4 x1 x2 )
\ Bug to catch: implementation reading x3 x4 instead of x1 x2

: main
  \ Use values that are close but distinguishable
  \ x1=100, x2=200, x3=101, x4=201
  \ If 2over grabs wrong pair, we get 101 201 instead of 100 200
  100 200 101 201 2over

  \ Stack now: 100 200 101 201 100 200 (TOS=200)
  \ Verify using subtraction/abs pattern

  200 - abs            \ TOS should be 200
  swap 100 - abs +     \ next should be 100
  swap 201 - abs +     \ x4 should be 201
  swap 101 - abs +     \ x3 should be 101
  swap 200 - abs +     \ x2 should be 200
  swap 100 - abs +     \ x1 should be 100
  \ Returns 0 if all matched, non-zero otherwise
;

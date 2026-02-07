\ adversarial-2over-similar.fs - Test 2over with similar values to catch offset bugs
\ Tests: If x1 x2 could be confused with x3 x4 due to wrong offset
\ Edge case: Adjacent values that differ by small amounts
\ expect: 1
\
\ 2over: ( x1 x2 x3 x4 -- x1 x2 x3 x4 x1 x2 )
\ Bug to catch: implementation reading x3 x4 instead of x1 x2

: main
  \ Use values that are close but distinguishable
  \ x1=100, x2=200, x3=101, x4=201
  \ If 2over grabs wrong pair, we get 101 201 instead of 100 200
  100 200 101 201 2over

  \ Stack now: 100 200 101 201 100 200 (TOS=200)
  \ Verify copied pair is 100 200, not 101 201

  200 =       \ TOS should be 200
  swap 100 =  \ next should be 100
  and

  \ Verify middle pair still 101 201
  swap 201 = and
  swap 101 = and

  \ Verify bottom pair still 100 200
  swap 200 = and
  swap 100 = and

  if 1 else 0 then
;

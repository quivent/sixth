\ expect: 3
\ Edge case: negative +LOOP step crossing zero boundary
\ Tests XOR boundary crossing logic specifically at zero
\ Step from 2 down to -4, crossing zero at different offsets
: main
  0
  -5 2 do          \ limit=-5, start=2
    1+
  -3 +loop
;
\ Trace: i=2 (1+), step -3 -> i=-1 (1+), step -3 -> i=-4 (1+), step -3 -> i=-7
\ After i=-4, new=-7 crosses limit=-5 boundary (from -4 to -7 crosses -5)
\ XOR check: (old-limit) XOR (new-limit) = (-4-(-5)) XOR (-7-(-5)) = 1 XOR -2 = -1 < 0 -> exit
\ count = 3

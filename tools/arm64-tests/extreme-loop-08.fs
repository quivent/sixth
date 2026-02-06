\ expect: 1
\ +LOOP that overshoots limit on first step
: main
  0
  5 0 do            \ limit=5, start=0
    1+
  100 +loop         \ step=100, immediately >= limit
;
\ First iteration: i=0, then +100 = 100 >= 5, exit
\ count = 1

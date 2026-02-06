\ expect: 6
\ Negative step +LOOP - count down from 10 to 0
\ Tests proper termination condition for negative step
: main
  0
  0 10 do             \ limit=0, start=10, step=-2
    1+
  -2 +loop
;
\ i=10: 1, i=8: 2, i=6: 3, i=4: 4, i=2: 5, i=0: 6
\ After i=0, step -2 makes i=-2 which crosses limit=0, exit
\ ANS Forth: +LOOP exits when index crosses limit boundary

\ adversarial-loop-15-minus1-cross.fs - Zero-crossing with +LOOP step -1
\ From 5 down toward -6: 5,4,3,2,1,0,-1,-2,-3,-4,-5,-6 = 12 iterations
\ (ANS Forth: loop terminates when crossing from limit-1 to limit)
\ expect: 12

: main
  0
  -6 5 do   \ from 5 down toward -6
    1+
    -1 +loop
;

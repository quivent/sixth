\ adversarial-loop-02-step-overshoot.fs - +LOOP with step > (limit - start)
\ Step 10, limit 5, start 0: should execute once and exit
\ expect: 1

: main
  0  \ accumulator
  5 0 do   \ limit=5, start=0
    1+
    10 +loop  \ step 10, immediately overshoots limit
  \ Result: 1 iteration
;

\ Adversarial DO-LOOP test: +loop with variable step
\ Step increases each iteration: step = i+1
\ i=0, step 1 -> i=1, step 2 -> i=3, step 4 -> i=7 >= 7, exit
\ Wait: i=0 (step=1), i=1 (step=2), i=3 (step=4), i=7 >= 7 exit
\ sum = 0+1+3 = 4
\ expect: 4
: main 0 7 0 do i + i 1 + +loop ;

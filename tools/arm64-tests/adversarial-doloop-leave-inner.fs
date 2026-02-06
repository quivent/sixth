\ Adversarial DO-LOOP test: leave from inner loop
\ Outer loop runs 3 times (j=0,1,2)
\ Inner: 5 0 do 1 + i 2 = if leave then loop
\ When inner i=2: 1+ executes BEFORE leave check, then leave triggers
\ Inner runs for i=0,1,2 = 3 iterations each outer iteration
\ Count = 3 outer * 3 inner = 9
\ expect: 9
: main 0 3 0 do 5 0 do 1 + i 2 = if leave then loop loop ;

\ Adversarial DO-LOOP test: leave from outer loop
\ When i=2, leave sets index=limit, causing outer loop to exit after inner loop
\ i=0: inner 3 times
\ i=1: inner 3 times
\ i=2: leave sets i=5, inner 3 times, then loop exits
\ Expected count = 9
\ NOTE: Use i not j - j only valid inside nested loop, here we check BEFORE inner DO
\ expect: 9
: main 0 5 0 do i 2 = if leave then 3 0 do 1 + loop loop ;

\ Adversarial DO-LOOP test: +loop that overshoots limit
\ Start at 0, limit 10, step 3
\ i values: 0, 3, 6, 9 (next would be 12 >= 10, so exit)
\ sum = 0+3+6+9 = 18
\ expect: 18
: main 0 10 0 do i + 3 +loop ;

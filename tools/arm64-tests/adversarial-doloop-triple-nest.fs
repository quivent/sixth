\ Adversarial DO-LOOP test: triple-nested loops
\ Count total iterations: 2 * 3 * 4 = 24
\ expect: 24
: main 0 2 0 do 3 0 do 4 0 do 1 + loop loop loop ;

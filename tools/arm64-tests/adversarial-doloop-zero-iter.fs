\ Adversarial DO-LOOP test: zero iterations (limit = start)
\ When limit equals start, loop body should not execute at all
\ expect: 42
: main 42 5 5 do 1 + loop ;

\ Adversarial DO-LOOP test: large iteration count (100+)
\ Count to 100, return count (not sum, to avoid exit code truncation)
\ expect: 100
: main 0 100 0 do 1 + loop ;

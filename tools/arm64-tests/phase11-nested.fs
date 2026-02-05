\ Phase 11 test: nested do loops with j
\ expect: 6
\ outer loop: 2 times (i=0,1), inner loop: 3 times each
\ count = 2 * 3 = 6
: main 0 2 0 do 3 0 do 1 + loop loop ;

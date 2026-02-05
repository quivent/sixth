\ Phase 11 test: j gets outer loop index
\ outer 0,1,2 each has inner 2 iterations, j sums outer index
\ i=0: j+j = 0+0 = 0; i=1: j+j = 1+1 = 2; i=2: j+j = 2+2 = 4
\ total: 0+2+4 = 6
\ expect: 6
: main 0 3 0 do 2 0 do j + loop loop ;

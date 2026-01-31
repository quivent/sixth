\ expect: 0 0 0 1 0 2 1 0 1 1 1 2
\ Test 638: nested do/loop printing i and j
: main 2 0 do 3 0 do j . i . loop loop cr ;

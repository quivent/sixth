\ expect: 0 1 2 1 2 3 2 3 4
\ Test 813: nested do loop with i and j
: main 3 0 do 3 0 do i j + . loop loop cr ;

\ expect: 0 1 2 1 2 3 2 3 4
\ Test 963: nested do/loop with j
: main 3 0 do 3 0 do i j + . loop loop cr ;

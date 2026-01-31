\ expect: 0 1 1 2 0 1 1 2
\ Test 825: nested 3-deep do loop
: main 2 0 do 2 0 do 2 0 do i j + . loop loop loop cr ;

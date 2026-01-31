\ expect: 1 2 3 2 4 6 3 6 9
\ Test 852: nested do loop printing multiplication table row
: main 4 1 do 4 1 do i j * . loop loop cr ;

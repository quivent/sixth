\ expect: 0 1 2 3 4 5 6 7 8
\ Test 848: three sequential do loops
: main 3 0 do i . loop 6 3 do i . loop 9 6 do i . loop cr ;

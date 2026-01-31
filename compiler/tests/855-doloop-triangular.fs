\ expect: 1 4 9 16 25
\ Test 855: do loop computing triangular numbers
: main 6 1 do 0 i 1+ 1 do j + loop . loop cr ;

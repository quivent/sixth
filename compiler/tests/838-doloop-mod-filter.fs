\ expect: 0 3 6 9 12
\ Test 838: do loop print multiples of 3
: main 15 0 do i 3 mod 0= if i . then loop cr ;

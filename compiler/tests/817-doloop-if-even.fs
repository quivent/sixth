\ expect: 0 2 4 6 8
\ Test 817: do loop skip odd print even
: main 10 0 do i 2 mod 0= if i . then loop cr ;

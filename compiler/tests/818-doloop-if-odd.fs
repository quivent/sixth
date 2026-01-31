\ expect: 1 3 5 7 9
\ Test 818: do loop skip even print odd
: main 10 0 do i 2 mod 0= if else i . then loop cr ;

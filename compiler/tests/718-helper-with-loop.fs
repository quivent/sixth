\ expect: 15
\ Test 718: helper that uses loop internally
: sum-to 0 swap 1+ 1 do i + loop ;
: main 5 sum-to . cr ;

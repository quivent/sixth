\ expect: 11 12 13
\ Test 564: 2dup inside a do loop
: main 4 1 do 10 i 2dup + . 2drop loop cr ;

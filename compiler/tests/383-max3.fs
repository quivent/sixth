\ expect: 7
\ Test 383: maximum of three numbers
: max 2dup > if drop else nip then ;
: main 5 3 max 7 max . cr ;

\ expect: 0 2 4 6 8
\ Test 820: do loop calling helper word
: double ( n -- n*2 ) 2* ;
: main 5 0 do i double . loop cr ;

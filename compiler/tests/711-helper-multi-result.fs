\ expect: 6 5
\ Test 711: word returns multiple results
: dup-inc ( n -- n n+1 ) dup 1+ ;
: main 5 dup-inc . . cr ;

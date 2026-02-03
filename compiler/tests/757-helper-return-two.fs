\ expect: -7 7
\ Test 757: helper returning two values both used
: dup-neg ( n -- n neg ) dup negate ;
: main ( -- ) 7 dup-neg . . cr ;

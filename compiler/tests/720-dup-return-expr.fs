\ expect: 10
\ Test 720: word with ( n -- n n ) used in expression
: dup-it ( n -- n n ) dup ;
: main 5 dup-it + . cr ;

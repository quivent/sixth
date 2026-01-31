\ expect: 1 4 9 16 25
\ Test 754: helper called 5 times
: sq dup * ;
: main 1 sq . 2 sq . 3 sq . 4 sq . 5 sq . cr ;

\ expect: 9 16
\ Test 713: helper called multiple times with different args
: sq dup * ;
: main 3 sq . 4 sq . cr ;

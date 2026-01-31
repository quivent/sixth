\ expect: 25
\ Test 744: helper taking three args
: mid3 ( a b c -- n ) rot drop + 2/ ;
: main 10 20 30 mid3 . cr ;

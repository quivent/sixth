\ expect: 99 42
\ Test 565: 2dup then 2drop should be identity
: main 42 99 2dup 2drop . . cr ;

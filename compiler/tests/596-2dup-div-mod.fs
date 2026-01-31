\ expect: 2 3
\ Test 596: 2dup then both div and mod
: main 17 5 2dup / rot rot mod . . cr ;

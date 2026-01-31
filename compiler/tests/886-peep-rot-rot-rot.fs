\ expect: 3 2 1
\ Test 886: rot rot rot = identity for 3 elements
: main 1 2 3 rot rot rot . . . cr ;

\ expect: 4 3 4 3 2 1
\ Test 585: 2dup with 4 items already on stack
: main 1 2 3 4 2dup . . . . . . cr ;

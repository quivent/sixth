\ expect: 7 12
\ Test 561: 2dup then multiply top two, add to originals
: main 3 4 2dup * rot rot + . . cr ;

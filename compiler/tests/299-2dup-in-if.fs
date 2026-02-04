\ expect: 8 5 3
\ Test: 2dup inside balanced if/else → 8 5 3
: main 3 5 1 if 2dup else 2dup then + . . . cr ;

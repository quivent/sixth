\ expect: 7 7 7 7 7
\ Test 1422: dup dup dup dup — grow stack to depth 5, print all
: main 7 dup dup dup dup . . . . . cr ;

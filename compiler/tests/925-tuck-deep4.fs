\ expect: 4 3 4 2 1
\ Test 925: tuck with 4 values
\ Stack: 1 2 3 4 -> tuck -> 1 2 4 3 4
: main 1 2 3 4 tuck . . . . . cr ;

\ expect: 32 4 3 2 1
\ Test 935: deep stack then 2* (shift left)
\ Stack: 1 2 3 4 16 -> 2* -> 1 2 3 4 32
: main 1 2 3 4 16 2* . . . . . cr ;

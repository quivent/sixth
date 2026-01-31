\ Test 932: deep stack then comparison
\ Stack: 5 6 7 10 3 -> > -> 5 6 7 -1 (10>3 is true=-1)
: main 5 6 7 10 3 > . . . . cr ;

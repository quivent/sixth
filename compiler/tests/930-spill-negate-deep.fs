\ expect: -50 40 30 20 10
\ Test 930: deep stack then negate
\ Stack: 10 20 30 40 50 -> negate -> 10 20 30 40 -50
: main 10 20 30 40 50 negate . . . . . cr ;

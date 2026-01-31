\ expect: 110 40 30 20 10
\ Test 912: 6 values on stack, add top 2, verify all 6 positions
\ Stack: 10 20 30 40 50 60 -> add top 2 -> 10 20 30 40 110
\ Print from top: 110 40 30 20 10
: main 10 20 30 40 50 60 + . . . . . cr ;

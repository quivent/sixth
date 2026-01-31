\ expect: 20 4 3 2 1
\ Test 554: five values on stack then divide preserves all
: main 1 2 3 4 100 5 / . . . . . cr ;

\ expect: 25 15 10 5
\ Test 916: deep stack then division (div clobbers rcx)
\ Stack: 5 10 15 100 4 -> / -> 5 10 15 25
: main 5 10 15 100 4 / . . . . cr ;

\ expect: -1 30 20 10
\ Test 934: deep stack then invert (bitwise NOT)
\ Stack: 10 20 30 0 -> invert -> 10 20 30 -1
: main 10 20 30 0 invert . . . . cr ;

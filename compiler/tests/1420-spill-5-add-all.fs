\ expect: 25
\ Test 1420: 5 values on stack, add them all — forces spills and reloads
\ 1 3 5 7 9 + + + + → 9+7=16, +5=21, +3=24, +1=25
: main 1 3 5 7 9 + + + + . cr ;

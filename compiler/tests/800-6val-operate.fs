\ Test 800: push 6 values operate and verify
\ 1 2 3 4 5 6: drop top 3, sum bottom 3: 1+2+3=6
: main 1 2 3 4 5 6 drop drop drop + + . cr ;

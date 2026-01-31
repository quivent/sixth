\ Test 921: 3 values, 2dup makes 5, arithmetic on top, verify bottom 3
\ Stack: 100 200 300 -> 2dup -> 100 200 300 200 300 -> + -> 100 200 300 500
: main 100 200 300 2dup + . . . . cr ;

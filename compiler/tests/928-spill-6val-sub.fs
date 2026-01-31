\ Test 928: 6 values, subtract top 2, verify rest
\ Stack: 1 2 3 4 50 10 -> - -> 1 2 3 4 40
: main 1 2 3 4 50 10 - . . . . . cr ;

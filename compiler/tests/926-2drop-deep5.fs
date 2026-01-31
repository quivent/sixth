\ Test 926: 2drop with 5 values (removes top 2, bottom 3 survive)
\ Stack: 10 20 30 40 50 -> 2drop -> 10 20 30
: main 10 20 30 40 50 2drop . . . cr ;

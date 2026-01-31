\ Test 915: 4 values, rot top 3, verify 4th untouched
\ Stack: 99 10 20 30 -> rot -> 99 20 30 10
\ Print: 10 30 20 99
: main 99 10 20 30 rot . . . . cr ;

\ expect: 20 40 30 10
\ Test 1424: rot on 4-deep stack — 3rd item at memory boundary
\ 10 20 30 40 rot → 10 30 40 20
\ Print: 20 40 30 10
: main 10 20 30 40 rot . . . . cr ;

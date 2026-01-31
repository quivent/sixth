\ expect: 20 10 40 30
\ Test 1427: manual 2swap via rot/>r/rot/r> — exchange pairs with spilled values
\ 10 20 30 40 → rot → 10 30 40 20 → >r → 10 30 40 R=[20]
\ rot → 30 40 10 → r> → 30 40 10 20
\ Print: 20 10 40 30
: main 10 20 30 40 rot >r rot r> . . . . cr ;

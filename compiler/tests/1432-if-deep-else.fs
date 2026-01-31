\ expect: 0 20 10
\ Test 1432: if/else takes else path with deep stack
\ 10 20 30 5 dup 30 > if drop 99 else drop 0 then nip
\ 5>30 false → else → drop 5 push 0 → 10 20 30 0 → nip → 10 20 0
\ Print: 0 20 10
: main 10 20 30 5 dup 30 > if drop 99 else drop 0 then nip . . . cr ;

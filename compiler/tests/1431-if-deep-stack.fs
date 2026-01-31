\ expect: 99 20 10
\ Test 1431: if/else with deep stack — branch with spilled values
\ 10 20 30 40 dup 30 > if drop 99 else drop 0 then nip
\ 40>30 true → drop 40 push 99 → 10 20 30 99 → nip → 10 20 99
\ Print: 99 20 10
: main 10 20 30 40 dup 30 > if drop 99 else drop 0 then nip . . . cr ;

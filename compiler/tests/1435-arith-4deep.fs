\ expect: 26
\ Test 1435: arithmetic on 4-deep stack — ops on values past rcx
\ 2 3 4 5 → * → 2 3 20 → + → 2 23 → + → 25
\ Wait: 4*5=20, 3+20=23, 2+23=25. That's only depth 4.
\ Deeper: 1 2 3 4 5 → over → 1 2 3 4 5 4
\ * → 1 2 3 4 20 → + → 1 2 3 24 → nip → 1 2 24 → + → 1 26 → nip → 26
: main 1 2 3 4 5 over * + nip + nip . cr ;

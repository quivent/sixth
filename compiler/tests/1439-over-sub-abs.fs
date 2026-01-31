\ expect: 7
\ Test 1439: over - abs — absolute difference between two values
\ 10 3 → over → 10 3 10 → - → 10 -7 → abs → 10 7 → nip → 7
: main 10 3 over - abs nip . cr ;

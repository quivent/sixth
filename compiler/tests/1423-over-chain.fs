\ expect: 10 20 10 20 10
\ Test 1423: over over over — deep copies stress NOS tracking
\ 10 20 over → 10 20 10
\ over → 10 20 10 20
\ over → 10 20 10 20 10
: main 10 20 over over over . . . . . cr ;

\ expect: 50 20 10
\ Test 1425: nip on deep stack — discard NOS when items spilled
\ 10 20 30 40 50 nip → 10 20 30 50
\ nip → 10 20 50
\ Print: 50 20 10
: main 10 20 30 40 50 nip nip . . . cr ;

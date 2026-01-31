\ expect: 50
\ Test 1436: nip nip — discard two NOS values rapidly
\ 10 20 30 40 50 → nip → 10 20 30 50 → nip → 10 20 50
\ nip → 10 50 → nip → 50
: main 10 20 30 40 50 nip nip nip nip . cr ;

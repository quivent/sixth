\ expect: 40 30 40 20 10
\ Test 1426: tuck on 4-deep stack — insert TOS below NOS
\ 10 20 30 40 tuck → 10 20 40 30 40 (depth=5)
\ Print: 40 30 40 20 10
: main 10 20 30 40 tuck . . . . . cr ;

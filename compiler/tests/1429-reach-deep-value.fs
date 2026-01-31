\ expect: 50 40 30 99 20 10
\ Test 1429: inject value into deep stack via return stack
\ 10 20 30 40 50 → >r >r >r → 10 20 R=[50,40,30]
\ 99 → 10 20 99 → r> r> r> → 10 20 99 30 40 50
\ Print: 50 40 30 99 20 10
: main 10 20 30 40 50 >r >r >r 99 r> r> r> . . . . . . cr ;

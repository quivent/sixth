\ expect: 15 8
\ Test 1437: tuck + pattern — common idiom ( a b -- b a+b )
\ 7 8 → tuck → 8 7 8 → + → 8 15
\ Print: 15 8
: main 7 8 tuck + . . cr ;

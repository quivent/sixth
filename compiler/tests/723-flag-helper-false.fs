\ expect: 0
\ Test 723: helper returning flag false path
: positive? 0 > ;
: main -3 positive? if 1 . else 0 . then cr ;

\ expect: 42
\ Test: value on stack survives else branch → 42
: main 42 0 if 99 . else then . cr ;

\ expect: 42
\ Test: nested if, inner branch taken → 42
: main 1 if 1 if 42 else 99 then else 0 then . cr ;

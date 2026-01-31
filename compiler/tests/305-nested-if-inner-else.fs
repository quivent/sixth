\ expect: 99
\ Test: nested if, inner else taken → 99
: main 1 if 0 if 42 else 99 then else 77 then . cr ;

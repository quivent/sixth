\ expect: 77
\ Test: nested if, outer else taken → 77
: main 0 if 1 if 42 else 99 then else 77 then . cr ;

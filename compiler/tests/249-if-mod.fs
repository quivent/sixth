\ expect: 42
\ Test: mod before if condition → 42
: main 10 3 mod 1 = if 42 else 99 then . cr ;

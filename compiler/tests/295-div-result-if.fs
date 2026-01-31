\ expect: 42
\ Test: division result as if condition input → 42
: main 6 2 / 3 = if 42 else 99 then . cr ;

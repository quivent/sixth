\ expect: 42
\ Test: 1+ before if condition → 42
: main 4 1+ 5 = if 42 else 99 then . cr ;

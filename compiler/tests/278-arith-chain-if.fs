\ expect: 42
\ Test: chained arithmetic before if → 42
: main 5 3 + 2 * 16 = if 42 else 99 then . cr ;

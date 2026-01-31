\ expect: 42
\ Test: negate before if condition → 42
: main -5 negate 0 > if 42 else 99 then . cr ;

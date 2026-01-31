\ expect: 42
\ Test: bitwise xor before if condition → 42
: main 5 3 xor 6 = if 42 else 99 then . cr ;

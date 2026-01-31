\ expect: 42
\ Test 454: dup < if - flags corruption regression
\ Expected output: 42
: main 5 dup 10 < if 42 else 99 then . cr ;

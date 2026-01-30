\ Test 455: dup = if - flags corruption regression
\ Expected output: 42
: main 5 dup 5 = if 42 else 99 then . cr ;

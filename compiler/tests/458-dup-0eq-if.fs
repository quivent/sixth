\ expect: 42
\ Test 458: dup 0= if - flags corruption regression
\ Expected output: 42
: main 0 dup 0= if drop 42 then . cr ;

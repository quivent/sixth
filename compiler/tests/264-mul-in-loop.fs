\ expect: 256
\ Test: 2* doubling to exact target → 256
: main 1 begin 2* dup 256 = until . cr ;

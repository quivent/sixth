\ expect: -1
\ Test 351: sign of negative
: sign dup 0 > if drop 1 else dup 0< if drop -1 else drop 0 then then ;
: main -42 sign . cr ;

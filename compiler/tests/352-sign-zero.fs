\ expect: 0
\ Test 352: sign of zero
: sign dup 0 > if drop 1 else dup 0< if drop -1 else drop 0 then then ;
: main 0 sign . cr ;

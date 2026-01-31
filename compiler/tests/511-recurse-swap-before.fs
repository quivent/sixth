\ expect: 55
\ Test 511: swap before recursive call
: rsum dup 0 > if dup 1- rsum + else drop 0 then ;
: main 5 10 swap drop rsum . cr ;

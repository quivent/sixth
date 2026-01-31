\ expect: 55
\ Test 758: recursive sum helper
: rsum dup 0 > if dup 1- recurse + else drop 0 then ;
: main 10 rsum . cr ;

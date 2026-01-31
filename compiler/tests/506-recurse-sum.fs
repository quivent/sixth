\ expect: 55
\ Test 506: recursive sum 1 to N
: rsum dup 0 > if dup 1- rsum + else drop 0 then ;
: main 10 rsum . cr ;

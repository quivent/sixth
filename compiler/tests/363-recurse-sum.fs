\ Test 363: recursive sum
: rsum dup 0 > if dup 1- rsum + then ;
: main 5 rsum . cr ;

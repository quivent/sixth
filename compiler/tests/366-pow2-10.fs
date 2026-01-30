\ Test 366: recursive power of 2
: pow2 dup 0= if drop 1 else 1- pow2 2* then ;
: main 10 pow2 . cr ;

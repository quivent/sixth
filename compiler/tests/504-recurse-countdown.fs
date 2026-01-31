\ expect: 5 4 3 2 1
\ Test 504: recursive countdown printing each value
: countdown dup 0 > if dup . 1- countdown else drop then ;
: main 5 countdown cr ;

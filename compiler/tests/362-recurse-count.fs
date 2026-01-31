\ expect: 3 2 1
\ Test 362: recursive countdown print
: countdown dup 0 > if dup . 1- countdown then ;
: main 3 countdown cr ;

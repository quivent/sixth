\ expect: 5 4 3 2 1
\ Test 527: print at each recursion level
: f dup 0 > if dup . 1- f else drop then ;
: main 5 f cr ;

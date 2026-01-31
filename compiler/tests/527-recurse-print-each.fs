\ Test 527: print at each recursion level
: f dup 0 > if dup . 1- f else drop then ;
: main 5 f cr ;

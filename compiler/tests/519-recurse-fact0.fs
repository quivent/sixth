\ Test 519: factorial base case 0
: fact dup 1 > if dup 1- fact * then ;
: main 0 fact . cr ;

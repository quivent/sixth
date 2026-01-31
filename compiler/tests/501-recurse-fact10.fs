\ Test 501: factorial of 10 via recursion
: fact dup 1 > if dup 1- fact * then ;
: main 10 fact . cr ;

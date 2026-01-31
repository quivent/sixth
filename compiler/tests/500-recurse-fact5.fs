\ expect: 120
\ Test 500: factorial of 5 via recursion
: fact dup 1 > if dup 1- fact * then ;
: main 5 fact . cr ;

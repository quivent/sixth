\ expect: 3628800
\ Test 321: factorial of 10
: fact dup 1 > if dup 1- fact * then ;
: main 10 fact . cr ;

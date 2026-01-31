\ expect: 720
\ Test 392: factorial of factorial
: fact dup 1 > if dup 1- fact * then ;
: main 3 fact fact . cr ;

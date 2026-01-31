\ Test 510: dup before recurse - stack manipulation pre-recursion
: fact dup 1 > if dup 1- fact * then ;
: main 6 dup fact swap drop . cr ;

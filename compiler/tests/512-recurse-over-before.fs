\ Test 512: over before recursive call preserves value below
: fact dup 1 > if dup 1- fact * then ;
: main 100 7 fact swap drop . cr ;

\ expect: 15
\ Test 513: recurse only in true branch of if
: f dup 0 > if dup 1- f + then ;
: main 5 f . cr ;

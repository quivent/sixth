\ expect: 19
\ Test 528: arithmetic after recursive call
: f dup 1 > if dup 1- f + 1+ then ;
: main 5 f . cr ;

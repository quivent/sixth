\ expect: 120
\ Test 517: non-tail recurse (work after recursive call)
: f dup 1 > if dup 1- f swap 1- * then ;
: main 6 f . cr ;

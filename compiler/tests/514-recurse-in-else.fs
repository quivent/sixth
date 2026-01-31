\ expect: 5
\ Test 514: recurse in else branch
: f dup 5 > if 1- else dup 1+ f swap drop then ;
: main 3 f . cr ;

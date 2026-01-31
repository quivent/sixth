\ expect: 30
\ Test 526: recurse inside nested if
: f dup 0 > if dup 3 > if 1- f else 1- f 10 + then then ;
: main 6 f . cr ;

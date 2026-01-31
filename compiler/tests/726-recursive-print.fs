\ expect: 3 2 1
\ Test 726: recursive word with side effects must not be DCEd
: countdown dup 0 > if dup . 1- recurse else drop then ;
: main 3 countdown cr ;

\ Test 967: exit in else branch of if
: check dup 0 > if . else drop 99 . cr exit then ;
: main 5 check cr ;

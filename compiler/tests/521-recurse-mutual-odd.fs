\ Test 521: mutual recursion - odd check returns 1
: odd dup 0 > if 1- even else drop 0 then ;
: even dup 0 > if 1- odd else drop 1 then ;
: main 7 odd . cr ;

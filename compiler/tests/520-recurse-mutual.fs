\ expect: 1
\ Test 520: mutual recursion - even/odd check
: odd dup 0 > if 1- even else drop 0 then ;
: even dup 0 > if 1- odd else drop 1 then ;
: main 10 even . cr ;

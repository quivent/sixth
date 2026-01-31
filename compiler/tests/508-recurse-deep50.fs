\ expect: 50
\ Test 508: deep recursion 50 levels
: deep dup 0 > if 1- deep 1+ else drop 0 then ;
: main 50 deep . cr ;

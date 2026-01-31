\ Test 507: deep recursion 20 levels
: deep dup 0 > if 1- deep 1+ else drop 0 then ;
: main 20 deep . cr ;

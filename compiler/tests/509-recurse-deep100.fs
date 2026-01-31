\ Test 509: deep recursion 100 levels
: deep dup 0 > if 1- deep 1+ else drop 0 then ;
: main 100 deep . cr ;

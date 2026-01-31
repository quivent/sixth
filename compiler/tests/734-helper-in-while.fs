\ Test 734: helper called in begin-while loop
: double 2* ;
: main 1 begin dup 100 < while double repeat . cr ;

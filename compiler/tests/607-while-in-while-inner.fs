\ Test 607: inner while loop runs 3 times per outer iteration (3*3=9)
: main 0 3 begin dup 0> while 3 begin dup 0> while 1- rot 1+ rot rot repeat drop 1- repeat drop . cr ;

\ expect: 8
\ Test 608: nested while - outer 4, inner 2 = 8 increments
: main 0 4 begin dup 0> while 2 begin dup 0> while 1- rot 1+ rot rot repeat drop 1- repeat drop . cr ;

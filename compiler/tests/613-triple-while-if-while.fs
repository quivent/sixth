\ Test 613: while { if { while } } triple nesting
: main 0 2 begin dup 0> while dup 1 > if 3 begin dup 0> while 1- rot 1+ rot rot repeat drop then 1- repeat drop . cr ;

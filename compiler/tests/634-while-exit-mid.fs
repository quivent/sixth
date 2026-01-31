\ Test 634: exit from middle of while loop
: main 1 begin dup 100 < while dup 7 = if . cr exit then 1+ repeat drop ;

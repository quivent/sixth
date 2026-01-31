\ expect: ABCABC
\ Test 704: emit in nested while loops
: main 2 begin dup 0> while 65 begin dup 67 <= while dup emit 1+ repeat drop 1- repeat drop cr ;

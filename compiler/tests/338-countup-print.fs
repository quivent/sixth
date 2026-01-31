\ expect: 1 2 3 4 5
\ Test 338: countup printing
: main 0 begin 1+ dup . dup 5 = until drop cr ;

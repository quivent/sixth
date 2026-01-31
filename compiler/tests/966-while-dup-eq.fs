\ expect: 0 1 2 3
\ Test 966: while loop with equality check as condition
\ Count up from 0, stop when equal to 4
: main 0 begin dup 4 <> while dup . 1+ repeat drop cr ;

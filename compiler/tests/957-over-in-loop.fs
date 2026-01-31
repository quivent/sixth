\ expect: 0 1 2
\ Test 957: over in loop body
\ Stack: limit counter -> over checks limit each iteration
: main 3 0 begin 2dup > while dup . 1+ repeat 2drop cr ;

\ expect: 0 1 2 3 4
\ Test 969: begin/again with conditional exit
: main 0 begin dup 5 < while dup . 1+ repeat drop cr ;

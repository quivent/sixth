\ expect: 0 1 2 3 2 1
\ Test 951: do/loop immediately followed by begin/while/repeat
: main 3 0 do i . loop 3 begin dup while dup . 1- repeat drop cr ;

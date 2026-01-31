\ expect: 5 4 3 2 1
\ Test 955: dup in while condition
: main 5 begin dup while dup . 1- repeat drop cr ;

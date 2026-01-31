\ expect: 5 4 3 2 1
\ Test 671: . inside while loop
: main 5 begin dup 0> while dup . 1- repeat drop cr ;

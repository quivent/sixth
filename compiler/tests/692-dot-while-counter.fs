\ expect: 3 2 1
\ Test 692: . inside while counts down
: main 3 begin dup 0> while dup . 1- repeat drop cr ;

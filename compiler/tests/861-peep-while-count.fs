\ expect: 5 4 3 2 1
\ Test 861: begin 1- dup while repeat countdown
: main 5 begin dup . 1- dup while repeat drop cr ;

\ expect: 5 4 3 2 1
\ Test 860: begin 1- dup 0= until countdown pattern
: main 5 begin dup . 1- dup 0= until drop cr ;

\ Test 948: begin/until where condition is complex expression
\ Count down from 5, stop when dup 2 * 0 = (i.e., when counter=0)
: main 5 begin dup . 1- dup 0= until drop cr ;

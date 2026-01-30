\ Test: negate inside loop body → -3 -2 -1
: main 3 begin dup 0 > while dup negate . 1- repeat drop cr ;

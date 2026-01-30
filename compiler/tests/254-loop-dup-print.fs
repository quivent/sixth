\ Test: dup and print inside while loop → 3 2 1
: main 3 begin dup 0 > while dup . 1- repeat drop cr ;

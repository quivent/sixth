\ Test 950: multiple begin/while/repeat in sequence (not nested)
: main 3 begin dup while dup . 1- repeat drop 3 begin dup while dup . 1- repeat drop cr ;

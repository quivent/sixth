\ expect: ***
\ Test: emit inside loop → prints ***
: main 3 begin dup 0 > while 42 emit 1- repeat drop cr ;

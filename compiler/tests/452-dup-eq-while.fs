\ expect: 3 2 1
\ Test 452: dup <> while - flags corruption regression
\ Expected output: 3 2 1
: main 3 begin dup 0 <> while dup . 1- repeat drop cr ;

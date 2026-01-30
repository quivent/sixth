\ Test 451: dup < while - flags corruption regression
\ Expected output: -3 -2 -1
: main -3 begin dup 0 < while dup . 1+ repeat drop cr ;

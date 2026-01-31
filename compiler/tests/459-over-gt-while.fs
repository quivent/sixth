\ expect: 10
\ Test 459: over with > while - flags corruption regression
\ Expected output: 10
: main 10 5 begin dup 0 > while 1- repeat drop . cr ;

\ expect: 0 1 2 3 4
\ Test 872: comparison before until with =
: main 0 begin dup . 1+ dup 5 = until drop cr ;

\ expect: 10 9 8 7 6 5 4 3 2 1
\ Test 388: print countdown from 10
: main 10 begin dup . 1- dup 0= until drop cr ;

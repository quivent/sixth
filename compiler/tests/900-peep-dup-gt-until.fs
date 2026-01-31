\ expect: 100 50 25 12 6 3 1
\ Test 900: dup comparison until loop
: main 100 begin dup . 2/ dup 0= until drop cr ;

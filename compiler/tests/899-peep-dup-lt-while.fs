\ expect: 1 2 4 8 16 32 64
\ Test 899: dup comparison while loop
: main 1 begin dup 100 < while dup . 2* repeat drop cr ;

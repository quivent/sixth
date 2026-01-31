\ expect: 2147483648
\ Test 774: large value arithmetic (64-bit, no 32-bit wrap)
: main 2147483647 1+ . cr ;

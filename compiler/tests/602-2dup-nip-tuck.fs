\ expect: 20 20 20 10
\ Test 602: 2dup then nip then tuck - full combo stress
: main 10 20 2dup nip tuck . . . . cr ;

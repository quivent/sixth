\ expect: 6 100
\ Test 750: helper must preserve values below its args
: inc 1+ ;
: main 100 5 inc . . cr ;

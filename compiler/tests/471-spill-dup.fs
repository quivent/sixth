\ expect: 9
\ Test 471: dup causing spill
\ Expected output: 9
: main 1 2 3 dup + + + . cr ;

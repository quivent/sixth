\ expect: 1
\ Test 749: helper result used in comparison
: double 2* ;
: main 5 double 10 = if 1 . else 0 . then cr ;

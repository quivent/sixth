\ expect: 80 30 20 10
\ Test 919: push 4 values, call helper, verify stack preserved
: double 2* ;
: main 10 20 30 40 double . . . . cr ;

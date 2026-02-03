\ expect: 25
\ Test 719: helper calls another helper
: sq ( n -- n^2 ) dup * ;
: sum-sq ( a b -- a^2+b^2 ) sq swap sq + ;
: main 3 4 sum-sq . cr ;

\ expect: 20
\ Test 719: helper calls another helper
: sq dup * ;
: sum-sq sq swap sq + ;
: main 3 4 sum-sq . cr ;

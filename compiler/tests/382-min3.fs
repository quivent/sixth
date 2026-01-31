\ expect: 3
\ Test 382: minimum of three numbers
: min ( a b -- c ) 2dup < if drop else nip then ;
: main 5 3 min 7 min . cr ;

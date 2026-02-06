\ expect: 42
\ Forward reference inside conditional branches
: test-fwd ( n -- n )
  dup 0< if neg-helper else pos-helper then ;
: neg-helper 100 ;
: pos-helper 42 ;
: main 5 test-fwd ;

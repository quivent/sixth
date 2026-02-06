\ expect: 100
\ Forward reference in conditional branch
: pick-path ( n -- n ) dup 0> if path-a else path-b then ;
: path-a 100 swap drop ;
: path-b 200 swap drop ;
: main 1 pick-path ;

\ Test 332: max of negative numbers
: max ( a b -- c ) 2dup > if drop else nip then ;
: main -3 -7 max . cr ;

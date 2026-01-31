\ Test 330: min of two numbers
: min ( a b -- c ) 2dup < if drop else nip then ;
: main 3 7 min . cr ;

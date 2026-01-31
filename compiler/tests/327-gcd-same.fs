\ Test 327: GCD of same number
: gcd ( a b -- g ) begin dup while tuck mod repeat drop ;
: main 42 42 gcd . cr ;

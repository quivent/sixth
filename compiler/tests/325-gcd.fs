\ expect: 4
\ Test 325: GCD of 12 and 8
: gcd ( a b -- g ) begin dup while tuck mod repeat drop ;
: main 12 8 gcd . cr ;

\ expect: 1
\ Test 326: GCD of coprimes
: gcd ( a b -- g ) begin dup while tuck mod repeat drop ;
: main 7 13 gcd . cr ;

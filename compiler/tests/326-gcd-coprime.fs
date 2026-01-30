\ Test 326: GCD of coprimes
: gcd begin dup while tuck mod repeat drop ;
: main 7 13 gcd . cr ;

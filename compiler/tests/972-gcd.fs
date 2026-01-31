\ expect: 6
\ Test 972: Euclidean GCD of 48 and 18 = 6
: gcd ( a b -- g ) begin dup while tuck mod repeat drop ;
: main 48 18 gcd . cr ;

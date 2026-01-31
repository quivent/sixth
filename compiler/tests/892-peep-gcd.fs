\ expect: 6
\ Test 892: GCD via euclidean algorithm
: gcd ( a b -- gcd ) begin dup while tuck mod repeat drop ;
: main 48 18 gcd . cr ;

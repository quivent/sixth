\ expect: 4 25 1
\ Test 996: GCD of multiple pairs
: gcd ( a b -- g ) begin dup while tuck mod repeat drop ;
: main 12 8 gcd . 100 75 gcd . 17 13 gcd . cr ;

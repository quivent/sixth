\ Test 910: GCD of coprime numbers
: gcd ( a b -- gcd ) begin dup while tuck mod repeat drop ;
: main 17 13 gcd . cr ;

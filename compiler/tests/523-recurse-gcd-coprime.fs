\ Test 523: recursive GCD of coprimes
: gcd dup 0 > if swap over mod gcd else drop then ;
: main 35 12 gcd . cr ;

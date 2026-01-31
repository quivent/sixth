\ Test 522: recursive GCD
: gcd dup 0 > if swap over mod gcd else drop then ;
: main 48 18 gcd . cr ;

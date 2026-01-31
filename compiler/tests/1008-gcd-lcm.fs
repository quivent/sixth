\ expect: 6 36
: gcd ( a b -- g ) begin dup while swap over mod repeat drop ;
: main 12 18 gcd . 12 6 / 18 * . cr ;

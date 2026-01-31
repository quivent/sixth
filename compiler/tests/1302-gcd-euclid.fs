\ expect: 6
: gcd ( a b -- g ) begin dup while tuck mod repeat drop ;
: main 48 18 gcd . cr ;

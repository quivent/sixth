\ Test 793: collatz-like starting at 2: 2 is >1, even, 2/=1, now 1 not >1, stop. print 1
: main 2 begin dup 1 > while dup 2 mod 0= if 2/ else dup 2* 1+ then repeat . cr ;

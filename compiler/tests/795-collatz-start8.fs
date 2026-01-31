\ Test 795: collatz start 8: 8->4->2->1
: main 8 begin dup 1 > while dup 2 mod 0= if 2/ else dup 2* 1+ then repeat . cr ;

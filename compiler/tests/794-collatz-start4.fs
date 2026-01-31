\ expect: 1
\ Test 794: collatz start 4: 4->2->1
: main 4 begin dup 1 > while dup 2 mod 0= if 2/ else dup 2* 1+ then repeat . cr ;

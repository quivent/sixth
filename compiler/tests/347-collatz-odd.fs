\ expect: 22
\ Test 347: collatz step on odd number
: collatz dup 2 mod 0= if 2/ else dup 2* + 1+ then ;
: main 7 collatz . cr ;

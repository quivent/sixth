\ Test 346: collatz step on even number
: collatz dup 2 mod 0= if 2/ else dup 2* + 1+ then ;
: main 6 collatz . cr ;

\ Test 992: count primes up to 20 = 8 (2,3,5,7,11,13,17,19)
: isprime dup 2 < if drop 0 exit then dup 2 = if drop -1 exit then dup 2 mod 0= if drop 0 exit then -1 swap dup 3 do dup i mod 0= if swap drop 0 swap then 2 +loop drop ;
: main 0 21 2 do i isprime if 1+ then loop . cr ;

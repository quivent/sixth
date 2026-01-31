\ Test 979: isPrime(97) prints -1 (true)
: isprime dup 2 < if drop 0 exit then dup 2 = if drop -1 exit then dup 2 mod 0= if drop 0 exit then -1 swap dup 3 do dup i mod 0= if swap drop 0 swap then 2 +loop drop ;
: main 97 isprime . cr ;

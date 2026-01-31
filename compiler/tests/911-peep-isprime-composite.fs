\ Test 911: primality check for composite 15
: isprime ( n -- flag ) dup 2 < if drop 0 exit then dup 2 = if drop 1 exit then dup 2 mod 0= if drop 0 exit then dup 3 begin 2dup dup * >= while 2dup mod 0= if 2drop drop 0 exit then 2+ repeat 2drop drop 1 ;
: main 15 isprime . cr ;

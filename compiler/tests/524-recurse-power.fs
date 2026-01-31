\ Test 524: recursive power (base^exp)
: power ( b e -- n ) dup 0 > if swap over swap 1- power * else drop drop 1 then ;
: main 2 10 power . cr ;

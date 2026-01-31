\ Test 643: if/else in while with both branches modifying acc
: main 0 10 begin dup 0> while dup 2 mod 0= if swap 1+ swap else swap 2 + swap then 1- repeat drop . cr ;

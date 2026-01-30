\ Test 379: count odd numbers 1-10
: main 0 10 begin dup 0 > while dup 2 mod 0= if swap else swap 1+ swap then 1- repeat drop . cr ;

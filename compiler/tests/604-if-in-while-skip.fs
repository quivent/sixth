\ Test 604: if inside while counts odd numbers only
: main 0 1 begin dup 10 < while dup 2 mod 0= if else swap 1+ swap then 1+ repeat drop . cr ;

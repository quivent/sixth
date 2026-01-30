\ Test: if inside while loop, count evens → 5
: main 0 10 begin dup 0 > while dup 2 mod 0= if swap 1+ swap then 1- repeat drop . cr ;

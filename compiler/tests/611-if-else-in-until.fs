\ Test 611: if/else inside begin/until - alternating add
: main 0 1 begin dup 2 mod 0= if swap 2 + swap else swap 1+ swap then 1+ dup 7 > until drop . cr ;

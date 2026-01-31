\ expect: 0
\ Test 610: while inside begin/until
: main 0 1 begin 3 begin dup 0> while 1- swap 1+ swap repeat drop 1+ dup 4 > until drop . cr ;

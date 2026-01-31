\ expect: 3
\ Test 606: while loop in else branch
: main 0 if 99 . cr else 0 3 begin dup 0> while 1- swap 1+ swap repeat drop . cr then ;

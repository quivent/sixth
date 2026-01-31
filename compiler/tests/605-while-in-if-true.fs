\ Test 605: while loop only runs in true branch
: main 1 if 0 5 begin dup 0> while 1- swap 1+ swap repeat drop . cr else 99 . cr then ;

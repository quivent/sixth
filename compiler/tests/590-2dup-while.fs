\ Test 590: 2dup inside while loop
: main 1 5 begin 2dup > while swap 1+ swap repeat . . cr ;

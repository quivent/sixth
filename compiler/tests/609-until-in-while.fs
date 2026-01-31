\ Test 609: begin/until inside while loop
: main 0 3 begin dup 0> while swap 1 begin 1+ dup 3 > until drop swap 1- repeat drop . cr ;

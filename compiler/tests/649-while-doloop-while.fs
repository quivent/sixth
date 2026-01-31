\ expect: 10
\ Test 649: while then doloop then while in sequence
: main 0 3 begin dup 0> while swap 1+ swap 1- repeat drop 5 0 do 1+ loop 2 begin dup 0> while swap 1+ swap 1- repeat drop . cr ;

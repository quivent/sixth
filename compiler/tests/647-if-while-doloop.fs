\ Test 647: if then while then doloop in sequence
: main 0 1 if 10 + then 3 begin dup 0> while swap 1+ swap 1- repeat drop 3 0 do 1+ loop . cr ;

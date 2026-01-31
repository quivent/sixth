\ expect: 9
\ Test 615: do/loop inside while
: main 0 3 begin dup 0> while swap 3 0 do 1+ loop swap 1- repeat drop . cr ;

\ expect: 5050
\ Test 990: sum 1 to 100 iteratively = 5050
: main 0 100 begin dup while dup rot + swap 1- repeat drop . cr ;

\ expect: ABCDE5
\ Test 702: emit in while, accumulator below preserved
: main 0 65 begin dup 70 < while dup emit swap 1+ swap 1+ repeat drop . cr ;

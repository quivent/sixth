\ Test: swap to maintain counter in loop → 5
: main 0 5 begin dup 0 > while swap 1+ swap 1- repeat drop . cr ;

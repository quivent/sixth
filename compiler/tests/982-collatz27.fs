\ Test 982: Collatz sequence length for 27 = 111 steps
: collatz 0 swap begin dup 1 > while dup 2 mod if 3 * 1+ else 2 / then swap 1+ swap repeat drop ;
: main 27 collatz . cr ;

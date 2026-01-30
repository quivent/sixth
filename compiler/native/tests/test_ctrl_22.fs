\ test_ctrl_22.fs - collatz steps
: main : collatz 0 swap begin dup 1 > while dup 2 mod 0= if 2 / else 3 * 1+ then swap 1+ swap repeat drop ;27 collatz . cr ;

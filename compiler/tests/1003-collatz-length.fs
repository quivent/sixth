: collatz-step ( n -- n' ) dup 2 mod 0= if 2 / else 3 * 1+ then ;
: collatz-len ( n -- count ) 0 swap begin dup 1 > while collatz-step swap 1+ swap repeat drop ;
: main 27 collatz-len . cr ;

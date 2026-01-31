\ expect: 8
: popcount ( n -- count ) 0 swap begin dup 0 > while dup 2 mod rot + swap 2 / repeat drop ;
: main 255 popcount . cr ;

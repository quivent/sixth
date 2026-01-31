\ expect: 15
: sum-digits ( n -- sum ) 0 swap begin dup 0 > while dup 10 mod rot + swap 10 / repeat drop ;
: main 12345 sum-digits . cr ;

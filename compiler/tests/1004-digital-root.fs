\ expect: 9
: sum-digits ( n -- sum ) 0 swap begin dup 0 > while dup 10 mod rot + swap 10 / repeat drop ;
: digital-root ( n -- root ) begin dup 9 > while sum-digits repeat ;
: main 9999 digital-root . cr ;

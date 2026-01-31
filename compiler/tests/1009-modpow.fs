\ expect: 24
: pow2 ( n -- 2^n ) 1 swap 0 do 2 * loop ;
: main 10 pow2 1000 mod . cr ;

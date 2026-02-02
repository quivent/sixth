\ expect: 14
\ Test: runtime value * 2 becomes shl
: double ( n -- n*2 ) 2 * ;
: main 7 double . cr ;

\ expected: 1500000000
\ Constant propagation through function calls

: add5 ( n -- n+5 ) 5 + ;
: mul3 ( n -- n*3 ) 3 * ;
: compute ( n -- result ) add5 mul3 ;

: main
  0 100000000 0 do
    i drop 0 compute +
  loop . cr ;

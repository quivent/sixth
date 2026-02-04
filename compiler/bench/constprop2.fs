\ expected: 3000000000
\ Constant folding across functions - nested constant expressions

: k1 ( -- n ) 7 3 + ;
: k2 ( -- n ) k1 2 * ;
: k3 ( -- n ) k2 k1 + ;

: main
  0 100000000 0 do k3 + loop . cr ;

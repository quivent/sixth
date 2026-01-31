\ expect: 42
: main
  -42 >r
  r@ 0< if r@ negate . else r@ . then
  r> drop cr ;

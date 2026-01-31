\ expect: 100 5
: main
  5 >r 100 >r
  r@ 50 > if r@ . then
  r> drop
  r@ 10 < if r@ . then
  r> drop cr ;

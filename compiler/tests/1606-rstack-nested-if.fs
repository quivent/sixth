\ expect: 20 10
: main
  10 >r 20 >r
  r@ 15 > if r@ . else 0 . then
  r> drop
  r@ 5 > if r@ . else 0 . then
  r> drop cr ;

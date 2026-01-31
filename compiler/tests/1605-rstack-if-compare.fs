\ expect: 50
: main
  50 >r
  r@ 30 > if r@ . else 0 . then
  r> drop cr ;

\ expected: 1000000000
\ Double-cell arithmetic stress

: main
  0. 1000000000 0 do 1. d+ loop drop . cr ;

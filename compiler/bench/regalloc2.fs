\ expected: 300000000
\ Live range splitting - values used at different points

: process ( c -- result )
  dup * 2 + ;

: main
  0 100000000 0 do
    1 process +
  loop . cr ;

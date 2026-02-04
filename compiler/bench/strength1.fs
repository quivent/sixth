\ expected: 800000000
\ Strength reduction - multiply to shift

: mul8 ( n -- n*8 ) 8 * ;

: main
  0 100000000 0 do i mul8 drop 8 + loop . cr ;

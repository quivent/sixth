\ expected: 50000000
\ Predictable branch pattern - CPU can predict

: classify ( n -- 0|1 )
  2 mod ;

: main
  0 100000000 0 do
    i classify +
  loop . cr ;

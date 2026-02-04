\ expected: 16666666
\ Division strength reduction - div by constant

: div3 ( n -- n/3 ) 3 / ;

: main
  0 100000000 0 do
    i div3 +
  loop 100000000 / . cr ;

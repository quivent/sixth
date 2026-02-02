\ expected: 1000000000
\ Pure call overhead stress - tiny word

: inc1 ( n -- n+1 ) 1+ ;

: main
  0 1000000000 0 do inc1 loop . cr ;

\ expected: 300000000
\ Tiny function called 100M times - GCC inlines completely

: inc3 ( n -- n+3 ) 1+ 1+ 1+ ;

: main
  0 100000000 0 do inc3 loop . cr ;

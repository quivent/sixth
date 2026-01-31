\ expect: 25
\ Euclidean distance squared between (1,2) and (4,6)
: sq ( n -- n*n ) dup * ;
: dist-sq ( x1 y1 x2 y2 -- d^2 )
  rot - sq        \ ( x1 x2 dy^2 )
  >r - sq r> + ;  \ dx^2 + dy^2
: main 1 2 4 6 dist-sq . cr ;

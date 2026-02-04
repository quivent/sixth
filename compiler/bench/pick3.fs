\ expected: 500000000
\ Pick from depth 3, 500M times
: main
  0 1 2 3                   ( acc d1 d2 d3 )
  500000000 0 do
    3 pick                  ( acc d1 d2 d3 acc )
    1 + >r drop drop drop r> ( acc' )
    1 2 3                   ( acc' d1 d2 d3 )
  loop
  drop drop drop . cr
;

\ expected: 200000000
\ Pick from depth 5, 200M times
: main
  0 1 2 3 4 5               ( acc d1 d2 d3 d4 d5 )
  200000000 0 do
    5 pick                  ( acc d1 d2 d3 d4 d5 acc )
    1 + >r drop drop drop drop drop r> ( acc' )
    1 2 3 4 5               ( acc' d1 d2 d3 d4 d5 )
  loop
  drop drop drop drop drop . cr
;

\ expected: 200000000
\ Pick from depth 4 - memory, 200M times
: main
  0 1 2 3 4                 ( acc d1 d2 d3 d4 )
  200000000 0 do
    4 pick                  ( acc d1 d2 d3 d4 acc )
    1 + >r drop drop drop drop r> ( acc' )
    1 2 3 4                 ( acc' d1 d2 d3 d4 )
  loop
  drop drop drop drop . cr
;

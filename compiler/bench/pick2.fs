\ expected: 500000000
\ Pick from depth 2, 500M times
: main
  0 1 2                     ( acc d1 d2 )
  500000000 0 do
    2 pick                  ( acc d1 d2 acc )
    1 + >r 2drop r>         ( acc' )
    1 2                     ( acc' d1 d2 )
  loop
  2drop . cr
;

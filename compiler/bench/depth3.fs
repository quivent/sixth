\ expected: 1000000000
\ Stack depth 3 throughout, 1B ops
: main
  0 0 0                     ( acc d1 d2 )
  1000000000 0 do
    rot 1 + -rot            ( acc' d1 d2 )
  loop
  2drop . cr
;

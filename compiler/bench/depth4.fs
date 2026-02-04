\ expected: 500000000
\ Stack depth 4 - first spill level, 500M ops
: main
  0 1 2 3                   ( acc d1 d2 d3 )
  500000000 0 do
    >r >r >r                ( acc ) ( R: d3 d2 d1 )
    1 +                     ( acc' )
    r> r> r>                ( acc' d1 d2 d3 )
  loop
  drop drop drop . cr
;

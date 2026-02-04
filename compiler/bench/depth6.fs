\ expected: 500000000
\ Stack depth 6, 500M ops
: main
  0 1 2 3 4 5               ( acc d1 d2 d3 d4 d5 )
  500000000 0 do
    >r >r >r >r >r          ( acc ) ( R: d5 d4 d3 d2 d1 )
    1 +                     ( acc' )
    r> r> r> r> r>          ( acc' d1 d2 d3 d4 d5 )
  loop
  drop drop drop drop drop . cr
;

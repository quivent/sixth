\ expected: 4500000000
\ Deep stack stress - forces register spills (5+ items)

: main
  0 1 2 3 4 500000000 0 do
    5 pick 4 pick 3 pick 2 pick over
    + + + + +
    5 +
  loop
  + + + + . cr ;

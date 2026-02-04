\ expected: 1000000000
\ Stack depth 2 throughout, 1B ops
: main
  0 0                       ( acc dummy )
  1000000000 0 do
    swap 1 + swap           ( acc' dummy )
  loop
  drop . cr
;

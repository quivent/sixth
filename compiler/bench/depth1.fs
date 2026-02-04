\ expected: 1000000000
\ Stack depth 1 throughout, 1B ops
: main
  0                         ( acc )
  1000000000 0 do
    1 +                     ( acc' )
  loop
  . cr
;

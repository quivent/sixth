\ expected: 500000000
\ Varying depth 1-8, 500M ops
: main
  0                         ( acc )
  500000000 0 do
    1 2 3 4 5 6 7           ( acc 1 2 3 4 5 6 7 ) depth 8
    drop drop drop drop drop drop drop  ( acc ) depth 1
    1 +                     ( acc' )
  loop
  . cr
;

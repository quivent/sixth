\ expected: 1000000000
\ Manually unrolled 2x, 500M iterations
: main
  0
  500000000 0 do
    1+ 1+
  loop
  . cr
;

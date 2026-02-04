\ expected: 1000000000
\ Manually unrolled 4x, 250M iterations
: main
  0
  250000000 0 do
    1+ 1+ 1+ 1+
  loop
  . cr
;

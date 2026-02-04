\ expected: 1000000000
\ Manually unrolled 8x, 125M iterations
: main
  0
  125000000 0 do
    1+ 1+ 1+ 1+ 1+ 1+ 1+ 1+
  loop
  . cr
;

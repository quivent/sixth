\ expected: 499999999500000000
\ DO/LOOP with i arithmetic, 1B iterations
: main
  0
  1000000000 0 do
    i +
  loop
  . cr
;

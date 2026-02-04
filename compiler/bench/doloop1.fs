\ expected: 1000000000
\ Simple DO/LOOP, 1B iterations
: main
  0
  1000000000 0 do
    1+
  loop
  . cr
;

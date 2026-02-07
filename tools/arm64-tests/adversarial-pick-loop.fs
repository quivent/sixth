\ expect: 5 4 3 2 1
\ Pick with varying index in a loop - brutal runtime index test
: main
  1 2 3 4 5
  5 0 do
    i pick . cr
  loop
  drop drop drop drop drop
;

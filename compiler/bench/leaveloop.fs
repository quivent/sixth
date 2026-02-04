\ expected: 100000000
\ DO/LOOP with LEAVE, 100M iterations
: main
  0
  200000000 0 do
    1+
    i 99999999 > if leave then
  loop
  . cr
;

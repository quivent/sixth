\ expect: 55
\ Test: DO-LOOP with stack growth each iteration
: main
  0
  11 1 do
    i +
  loop
;

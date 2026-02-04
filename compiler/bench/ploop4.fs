\ expected: 100000000
\ +LOOP variable step (1 + i/100000000), 100M iterations
: main
  0
  1000000000 0 do
    1+
    i 100000000 / 1+
  +loop
  . cr
;

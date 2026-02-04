\ expected: 500000000
\ +LOOP negative step countdown, 500M iterations
: main
  0
  0 1000000000 do
    1+
  -2 +loop
  . cr
;

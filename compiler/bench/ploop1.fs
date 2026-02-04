\ expected: 500000000
\ +LOOP step 2, 500M iterations
: main
  0
  1000000000 0 do
    1+
  2 +loop
  . cr
;

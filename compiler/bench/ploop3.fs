\ expected: 142857143
\ +LOOP step 7, 143M iterations
: main
  0
  1000000000 0 do
    1+
  7 +loop
  . cr
;

\ expected: 333333334
\ +LOOP step 3, 333M iterations
: main
  0
  1000000000 0 do
    1+
  3 +loop
  . cr
;

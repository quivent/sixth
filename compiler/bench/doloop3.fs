\ expected: 1000000000
\ Nested DO/LOOP 2 deep, 100K x 10K
: main
  0
  100000 0 do
    10000 0 do
      1+
    loop
  loop
  . cr
;

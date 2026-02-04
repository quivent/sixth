\ expected: 100000000
\ Nested DO/LOOP 4 deep, 100 x 100 x 100 x 100
: main
  0
  100 0 do
    100 0 do
      100 0 do
        100 0 do
          1+
        loop
      loop
    loop
  loop
  . cr
;

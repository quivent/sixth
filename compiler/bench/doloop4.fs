\ expected: 1000000000
\ Nested DO/LOOP 3 deep, 1K x 1K x 1K
: main
  0
  1000 0 do
    1000 0 do
      1000 0 do
        1+
      loop
    loop
  loop
  . cr
;

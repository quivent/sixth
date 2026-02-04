\ expected: 100000000
\ Nested loop using J, 10K x 10K
: main
  0
  10000 0 do
    10000 0 do
      j i + drop
      1+
    loop
  loop
  . cr
;

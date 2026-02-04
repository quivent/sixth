\ expected: 4999999950000000
\ Nested loop stress - exercises j register

: main
  0 100000 0 do
    100000 0 do
      j i + +
    loop
  loop . cr ;

\ expected: 3150000000
\ Register pressure - multiple live values

: main
  0 100000000 0 do
    7 14 + 7 + i 8 mod + +
  loop . cr ;

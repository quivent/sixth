\ expected: 750000000
\ Comparison operators stress - <, >, 0=, 0<, >=

: main
  0 1000000000 0 do
    i 3 and 0= if 1+ then
    i 4 and 0> if 1+ then
    i 500000000 < if 1+ then
  loop . cr ;

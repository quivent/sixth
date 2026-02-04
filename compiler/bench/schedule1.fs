\ expected: 1300000000
\ Instruction scheduling - independent ops

: main
  0 100000000 0 do
    4 4 + 5 + +
  loop . cr ;

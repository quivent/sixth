\ Count to 1 billion benchmark - tight loop
: main
  0    \ sum
  1000000000 0 do
    i 1 and +
  loop
  . cr ;

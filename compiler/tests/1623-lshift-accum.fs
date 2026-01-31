\ expect: 255
\ OR together 1<<0 | 1<<1 | ... | 1<<7 = 255
: main
  0
  8 0 do
    1 i lshift or
  loop
  . cr ;

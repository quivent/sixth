\ expect: 10 11
\ 186 = 0xBA = 1011 1010
\ Low nibble: 186 & 15 = 10 (0xA)
\ High nibble: 186 >> 4 & 15 = 11 (0xB)
: main
  186
  dup 15 and .
  4 rshift 15 and .
  cr ;

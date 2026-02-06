\ expect: 0
\ Test: Boolean operations AND, OR, XOR with flag values

: main
  \ AND tests - bitwise, so -1 AND -1 = -1, -1 AND 0 = 0
  -1 -1 and -1 <> if 1 exit then            \ true AND true = true
  -1 0 and 0 <> if 2 exit then              \ true AND false = false
  0 -1 and 0 <> if 3 exit then              \ false AND true = false
  0 0 and 0 <> if 4 exit then               \ false AND false = false

  \ OR tests
  -1 -1 or -1 <> if 5 exit then             \ true OR true = true
  -1 0 or -1 <> if 6 exit then              \ true OR false = true
  0 -1 or -1 <> if 7 exit then              \ false OR true = true
  0 0 or 0 <> if 8 exit then                \ false OR false = false

  \ XOR tests
  -1 -1 xor 0 <> if 9 exit then             \ true XOR true = false
  -1 0 xor -1 <> if 10 exit then            \ true XOR false = true
  0 -1 xor -1 <> if 11 exit then            \ false XOR true = true
  0 0 xor 0 <> if 12 exit then              \ false XOR false = false

  0
;

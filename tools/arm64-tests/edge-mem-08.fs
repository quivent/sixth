\ expect: 64
\ EDGE: HERE with comma (,) and c-comma (c,) operations
\ Tests: Build data structure in heap using , and c, then read back

: main
  here            \ Save initial here position
  42 ,            \ Store cell (8 bytes)
  65 c,           \ Store byte 'A'
  66 c,           \ Store byte 'B'
  67 c,           \ Store byte 'C'
  \ here should have advanced by 8 + 3 = 11 bytes

  \ Read back the cell we stored
  dup @           \ Should be 42

  \ Read the bytes (at offset 8 from saved here)
  swap 8 + c@     \ Should be 65 (A)

  \ Combine: (42 + 65) / 2 + 10 = 107/2 + 10 = 53 + 10 = 63...
  \ Let's simplify: just return first byte
  swap drop       \ Drop the 42, keep 65
  1 -             \ 65 - 1 = 64
;

\ expect: 255
\ EDGE: Byte operations at cell boundaries with c@ and c!
\ Tests: Write and read individual bytes at various offsets
\ This tests byte addressing vs cell addressing alignment

create bytes 16 allot

: main
  \ Write individual bytes at each offset 0-7
  11 bytes 0 + c!
  22 bytes 1 + c!
  33 bytes 2 + c!
  44 bytes 3 + c!
  55 bytes 4 + c!
  66 bytes 5 + c!
  77 bytes 6 + c!
  88 bytes 7 + c!

  \ Read back and sum (should be 11+22+33+44+55+66+77+88 = 396)
  bytes 0 + c@
  bytes 1 + c@ +
  bytes 2 + c@ +
  bytes 3 + c@ +
  bytes 4 + c@ +
  bytes 5 + c@ +
  bytes 6 + c@ +
  bytes 7 + c@ +

  \ Truncate to fit in exit code
  141 -    \ 396 - 141 = 255
;

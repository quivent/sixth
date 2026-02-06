\ expect: 1
\ ADVERSARIAL: Large fill (128 bytes)
\ Tests the loop can handle many iterations without corruption
\ Verifies first, middle, and last bytes are all filled

: main
  here 128 allot      \ allocate 128 bytes
  here 128 - dup      \ save start address
  128 66 fill         \ fill with 'B' (66)

  \ Check first byte
  dup c@ 66 =
  \ Check middle byte (offset 64)
  over 64 + c@ 66 = and
  \ Check last byte (offset 127)
  swap 127 + c@ 66 = and
  if 1 else 0 then
;

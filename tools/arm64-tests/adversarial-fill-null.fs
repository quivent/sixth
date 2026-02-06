\ expect: 1
\ ADVERSARIAL: Fill with null bytes (0x00)
\ Tests that 0 is handled as a valid byte value
\ Null bytes are tricky - they can terminate strings early

: main
  here 8 allot        \ allocate 8 bytes
  here 8 -            \ start address
  dup 8 255 fill      \ first fill with 0xFF
  dup 8 0 fill        \ now fill with 0x00

  \ Check all bytes are 0
  dup c@ 0 =
  over 1 + c@ 0 = and
  over 7 + c@ 0 = and
  swap drop
  if 1 else 0 then
;

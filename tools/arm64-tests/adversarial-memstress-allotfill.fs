\ expect: 255
\ ADVERSARIAL: Use here/allot then fill, then read back all bytes
\ Tests that fill writes to all allocated bytes correctly
: main
  here           \ save start address
  16 allot       \ allocate 16 bytes
  dup 16 255 fill   \ fill with 0xFF
  \ verify all 16 bytes are 0xFF - if any isn't, exit with wrong value
  dup c@         \ byte 0
  over 1+ c@ and \ byte 1
  over 2 + c@ and
  over 3 + c@ and
  over 4 + c@ and
  over 5 + c@ and
  over 6 + c@ and
  over 7 + c@ and
  over 8 + c@ and
  over 9 + c@ and
  over 10 + c@ and
  over 11 + c@ and
  over 12 + c@ and
  over 13 + c@ and
  over 14 + c@ and
  swap 15 + c@ and   \ byte 15 - all must be 255, ANDed = 255
;

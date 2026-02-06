\ expect: 0
\ Test: FILL operation using multiple variables as buffer
\ Verify correct byte filling of memory regions

\ Use 8 variables as a 64-byte buffer (8 cells x 8 bytes)
variable b0 variable b1 variable b2 variable b3
variable b4 variable b5 variable b6 variable b7
variable fail-code

: main
  0 fail-code !

  \ Fill 64 bytes starting at b0 with 170 (0xAA)
  b0 64 170 fill

  \ Check first cell
  b0 c@ 170 <> if 1 fail-code ! then

  \ Check a middle byte
  fail-code @ 0= if b0 32 + c@ 170 <> if 2 fail-code ! then then

  \ Check last byte of region
  fail-code @ 0= if b0 63 + c@ 170 <> if 3 fail-code ! then then

  \ Fill subset with different value (85 = 0x55)
  fail-code @ 0= if
    b0 10 + 20 85 fill

    \ Check boundaries preserved
    b0 9 + c@ 170 <> if 4 fail-code ! then
  then
  fail-code @ 0= if b0 10 + c@ 85 <> if 5 fail-code ! then then
  fail-code @ 0= if b0 29 + c@ 85 <> if 6 fail-code ! then then
  fail-code @ 0= if b0 30 + c@ 170 <> if 7 fail-code ! then then

  \ Fill with 0
  fail-code @ 0= if
    b0 64 0 fill
    b0 c@ 0<> if 8 fail-code ! then
  then
  fail-code @ 0= if b0 63 + c@ 0<> if 9 fail-code ! then then

  fail-code @ ;

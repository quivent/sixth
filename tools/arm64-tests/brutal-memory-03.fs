\ expect: 0
\ Test: C@ and C! byte operations
\ Verify byte isolation within a cell

variable buf
variable fail-code

: main
  0 fail-code !
  0 buf !

  \ Store byte at base address (0xAA = 170)
  170 buf c!
  buf c@ 170 <> if 1 fail-code ! then

  \ Check only low byte affected (high bytes should be 0)
  fail-code @ 0= if buf @ 170 <> if 2 fail-code ! then then

  \ Store different byte (0x55 = 85)
  fail-code @ 0= if
    85 buf c!
    buf c@ 85 <> if 3 fail-code ! then
  then

  \ Test that c@ masks to 8 bits (0xFF = 255)
  fail-code @ 0= if
    255 buf c!
    buf c@ 255 <> if 4 fail-code ! then
  then

  \ Negative byte stored should read as unsigned
  fail-code @ 0= if
    -1 buf c!
    buf c@ 255 <> if 5 fail-code ! then
  then

  fail-code @ ;

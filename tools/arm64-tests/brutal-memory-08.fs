\ expect: 0
\ Test: CELLS and CELL+ address arithmetic
\ Verify correct scaling factors (8 bytes on 64-bit)

variable fail-code

: main
  0 fail-code !

  \ CELLS should multiply by cell size (8 on 64-bit)
  1 cells 8 <> if 1 fail-code ! then
  fail-code @ 0= if 2 cells 16 <> if 2 fail-code ! then then
  fail-code @ 0= if 10 cells 80 <> if 3 fail-code ! then then

  \ CELL+ should add one cell
  fail-code @ 0= if 100 cell+ 108 <> if 4 fail-code ! then then
  fail-code @ 0= if 0 cell+ 8 <> if 5 fail-code ! then then

  \ Multiple CELL+ operations
  fail-code @ 0= if 0 cell+ cell+ 16 <> if 6 fail-code ! then then
  fail-code @ 0= if 0 cell+ cell+ cell+ 24 <> if 7 fail-code ! then then

  \ CELLS with 0
  fail-code @ 0= if 0 cells 0<> if 8 fail-code ! then then

  \ Larger CELLS
  fail-code @ 0= if 100 cells 800 <> if 9 fail-code ! then then

  fail-code @ ;

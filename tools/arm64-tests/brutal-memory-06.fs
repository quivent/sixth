\ expect: 0
\ Test: MOVE with overlapping regions (forward overlap)
\ When dst > src, must copy backward to avoid corruption
\ Note: Uses flag variable instead of EXIT in IF to work around compiler bug

\ 8 cells = 64 bytes
variable r0 variable r1 variable r2 variable r3
variable r4 variable r5 variable r6 variable r7
variable fail-code

: main
  0 fail-code !

  \ Initialize with known pattern: 0 1 2 3 4 5 6 7 ... 63
  64 0 do i r0 i + c! loop

  \ Move bytes 0-31 to bytes 8-39 (overlapping by 24 bytes)
  \ src=r0, dst=r0+8, count=32
  r0 r0 8 + 32 move

  \ After move, bytes 8-39 should contain 0 1 2 3 ... 31
  32 0 do
    fail-code @ 0= if
      r0 8 + i + c@ i <> if i 100 + fail-code ! then
    then
  loop

  \ Bytes 0-7 should still be 0 1 2 3 4 5 6 7
  8 0 do
    fail-code @ 0= if
      r0 i + c@ i <> if i 200 + fail-code ! then
    then
  loop

  fail-code @ ;

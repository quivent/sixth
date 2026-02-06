\ expect: 0
\ Test: MOVE with overlapping regions (backward overlap)
\ When dst < src, forward copy is safe

\ 8 cells = 64 bytes
variable r0 variable r1 variable r2 variable r3
variable r4 variable r5 variable r6 variable r7
variable fail-code

: main
  0 fail-code !

  \ Initialize: put 100,101,102... starting at offset 16
  32 0 do 100 i + r0 16 + i + c! loop

  \ Move bytes 16-47 to bytes 8-39 (overlapping backward)
  \ src=r0+16, dst=r0+8, count=32
  r0 16 + r0 8 + 32 move

  \ After move, bytes 8-39 should contain 100 101 102 ... 131
  32 0 do
    fail-code @ 0= if
      r0 8 + i + c@ 100 i + <> if i 100 + fail-code ! then
    then
  loop

  fail-code @ ;

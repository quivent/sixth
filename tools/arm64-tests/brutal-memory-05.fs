\ expect: 0
\ Test: MOVE with non-overlapping regions
\ Forward copy semantics

\ Source buffer: 4 cells = 32 bytes
variable s0 variable s1 variable s2 variable s3
\ Destination buffer: 4 cells = 32 bytes
variable d0 variable d1 variable d2 variable d3
variable fail-code

: main
  0 fail-code !

  \ Initialize src with pattern 0,1,2,3...31
  32 0 do i s0 i + c! loop

  \ Clear dst
  d0 32 0 fill

  \ Move src to dst
  s0 d0 32 move

  \ Verify all bytes copied
  32 0 do
    fail-code @ 0= if
      d0 i + c@ i <> if i 100 + fail-code ! then
    then
  loop

  \ Verify src unchanged
  32 0 do
    fail-code @ 0= if
      s0 i + c@ i <> if i 200 + fail-code ! then
    then
  loop

  fail-code @ ;

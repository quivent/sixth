\ expect: 0
\ Test: C@ and C! at cell boundaries - byte access stress
\ Tests alignment handling for byte operations

variable cells8
variable ok

: setup here cells8 ! 64 allot ;

: fill-cells ( -- )
  8 0 do
    i 16 * 1+ cells8 @ i 8 * + !
  loop ;

: check-bytes ( -- flag )
  cells8 @ c@ 1 <> if 0 exit then
  cells8 @ 1+ c@ 0 <> if 0 exit then
  cells8 @ 8 + c@ 17 <> if 0 exit then
  cells8 @ 16 + c@ 33 <> if 0 exit then
  1 ;

: byte-modify ( -- )
  99 cells8 @ c!
  88 cells8 @ 1+ c!
  77 cells8 @ 2 + c! ;

: main
  setup
  fill-cells
  check-bytes 0= if 1 exit then
  byte-modify
  cells8 @ c@ 99 <> if 2 exit then
  cells8 @ 1+ c@ 88 <> if 3 exit then
  cells8 @ 2 + c@ 77 <> if 4 exit then
  0 ;

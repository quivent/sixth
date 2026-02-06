\ expect: 0
\ Test: Overlapping MOVE operations - forward and backward
\ This tests whether MOVE handles overlapping regions correctly

variable src
variable dst
variable ok

: setup here src ! 256 allot here dst ! 256 allot ;

: init-src ( -- )
  256 0 do i src @ i + c! loop ;

: test-move ( -- flag )
  init-src
  src @ dst @ 256 move
  1 ok !
  256 0 do
    dst @ i + c@ i <> if 0 ok ! then
  loop ok @ ;

: test-overlap ( -- flag )
  64 0 do i 1+ src @ i + c! loop
  src @ 8 + src @ 32 move
  1 ok !
  32 0 do
    src @ i + c@ i 9 + <> if 0 ok ! then
  loop ok @ ;

: main
  setup
  test-move 0= if 1 exit then
  test-overlap 0= if 2 exit then
  0 ;

\ expect: 0
\ Test: FILL with various patterns and sizes
\ Stress test memory fill operation

variable buf1
variable buf2
variable ok

: setup here buf1 ! 256 allot here buf2 ! 256 allot ;

: test-fill-ff ( -- flag )
  buf1 @ 256 255 fill
  1 ok !
  256 0 do
    buf1 @ i + c@ 255 <> if 0 ok ! then
  loop ok @ ;

: test-fill-00 ( -- flag )
  buf1 @ 256 0 fill
  1 ok !
  256 0 do
    buf1 @ i + c@ 0<> if 0 ok ! then
  loop ok @ ;

: test-fill-aa ( -- flag )
  buf1 @ 256 170 fill
  1 ok !
  256 0 do
    buf1 @ i + c@ 170 <> if 0 ok ! then
  loop ok @ ;

: test-partial ( -- flag )
  buf2 @ 256 0 fill
  buf2 @ 64 + 64 99 fill
  1 ok !
  64 0 do
    buf2 @ i + c@ 0<> if 0 ok ! then
  loop
  64 0 do
    buf2 @ 64 + i + c@ 99 <> if 0 ok ! then
  loop
  128 0 do
    buf2 @ 128 + i + c@ 0<> if 0 ok ! then
  loop ok @ ;

: main
  setup
  test-fill-ff 0= if 1 exit then
  test-fill-00 0= if 2 exit then
  test-fill-aa 0= if 3 exit then
  test-partial 0= if 4 exit then
  0 ;

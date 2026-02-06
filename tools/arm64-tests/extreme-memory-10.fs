\ expect: 0
\ Test: Edge cases - zero-length operations, single byte, max values
\ Adversarial edge case testing

variable edge
variable tmp

: setup here edge ! 64 allot ;

: test-zero-len ( -- flag )
  edge @ 0 0 fill
  edge @ edge @ 0 move
  1 ;

: test-single ( -- flag )
  255 edge @ c!
  edge @ c@ 255 <> if 0 exit then
  0 edge @ c!
  edge @ c@ 0<> if 0 exit then
  1 ;

: test-max-val ( -- flag )
  -1 tmp !
  tmp @ -1 <> if 0 exit then
  tmp @ 0 < 0= if 0 exit then
  1 ;

: test-addr-arith ( -- flag )
  edge @ tmp !
  tmp @ 32 + edge @ 32 + <> if 0 exit then
  tmp @ 1- edge @ 1- <> if 0 exit then
  1 ;

: test-cell-bnd ( -- flag )
  12345678 edge @ !
  edge @ c@ 78 and 78 <> if 0 exit then
  87654321 edge @ 8 + !
  edge @ 8 + c@ 177 and 177 <> if 0 exit then
  1 ;

: main
  setup
  test-zero-len 0= if 1 exit then
  test-single 0= if 2 exit then
  test-max-val 0= if 3 exit then
  test-addr-arith 0= if 4 exit then
  test-cell-bnd 0= if 5 exit then
  0 ;

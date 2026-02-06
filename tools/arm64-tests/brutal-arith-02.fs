\ expect: 0
\ Test: Signed underflow with subtraction near MIN-INT
\ MIN-INT - 1 should wrap to MAX-INT

: max-int -1 1 rshift ;
: min-int 1 63 lshift ;

: test1
  \ Subtracting 1 from MIN-INT should wrap to MAX-INT
  min-int 1 - max-int = 0= if 1 exit then
  0 ;

: test2
  \ MIN-INT - MIN-INT should be 0
  min-int min-int - 0= 0= if 2 exit then
  0 ;

: test3
  \ 0 - MIN-INT should be MIN-INT (overflow case!)
  \ Because -MIN-INT cannot be represented in signed 64-bit
  0 min-int - min-int = 0= if 3 exit then
  0 ;

: main
  test1 dup 0<> if exit then drop
  test2 dup 0<> if exit then drop
  test3 ;

\ expect: 0
\ Test: Signed overflow with addition near MAX-INT
\ MAX_INT64 + 1 should wrap to MIN_INT64 (two's complement)
\ MAX_INT64 = -1 1 rshift (logical shift clears sign bit)
\ MIN_INT64 = 1 63 lshift (only sign bit set)

: max-int -1 1 rshift ;
: min-int 1 63 lshift ;

: test1
  \ Adding 1 to MAX-INT should wrap to MIN-INT
  max-int 1 + min-int = 0= if 1 exit then
  0 ;

: test2
  \ Adding MAX-INT to MAX-INT should give -2
  max-int max-int + -2 = 0= if 2 exit then
  0 ;

: test3
  \ MAX-INT + MAX-INT + 2 should be 0 (full wrap)
  max-int max-int + 2 + 0= 0= if 3 exit then
  0 ;

: main
  test1 dup 0<> if exit then drop
  test2 dup 0<> if exit then drop
  test3 ;

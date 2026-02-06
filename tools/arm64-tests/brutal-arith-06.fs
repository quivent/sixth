\ expect: 0
\ Test: Comparisons and conditional selection as MIN/MAX substitutes
\ The compiler lacks min/max, so we test the comparison operators

: max-int -1 1 rshift ;
: min-int 1 63 lshift ;

\ Manual min: ( a b -- min )
: min 2dup < if drop else nip then ;
\ Manual max: ( a b -- max )
: max 2dup > if drop else nip then ;

: test1
  \ Basic min
  3 5 min 3 = 0= if 1 exit then
  5 3 min 3 = 0= if 2 exit then
  -3 5 min -3 = 0= if 3 exit then
  5 -3 min -3 = 0= if 4 exit then
  0 ;

: test2
  \ Basic max
  3 5 max 5 = 0= if 5 exit then
  5 3 max 5 = 0= if 6 exit then
  -3 5 max 5 = 0= if 7 exit then
  5 -3 max 5 = 0= if 8 exit then
  0 ;

: test3
  \ Edge cases with MIN-INT and MAX-INT
  min-int max-int min min-int = 0= if 9 exit then
  min-int max-int max max-int = 0= if 10 exit then
  0 ;

: test4
  \ MIN-INT is smallest signed value
  min-int 0 min min-int = 0= if 11 exit then
  min-int -1 min min-int = 0= if 12 exit then
  0 ;

: test5
  \ MAX-INT is largest signed value
  max-int 0 max max-int = 0= if 13 exit then
  max-int -1 max max-int = 0= if 14 exit then
  0 ;

: main
  test1 dup 0<> if exit then drop
  test2 dup 0<> if exit then drop
  test3 dup 0<> if exit then drop
  test4 dup 0<> if exit then drop
  test5 ;

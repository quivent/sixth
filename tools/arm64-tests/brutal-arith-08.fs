\ expect: 0
\ Test: Multiplication overflow and edge cases

: max-int -1 1 rshift ;
: min-int 1 63 lshift ;

: test1
  \ Basic multiplication
  3 5 * 15 = 0= if 1 exit then
  -3 5 * -15 = 0= if 2 exit then
  3 -5 * -15 = 0= if 3 exit then
  -3 -5 * 15 = 0= if 4 exit then
  0 ;

: test2
  \ Multiplication by 0
  0 max-int * 0= 0= if 5 exit then
  min-int 0 * 0= 0= if 6 exit then
  0 ;

: test3
  \ Multiplication by 1 and -1
  max-int 1 * max-int = 0= if 7 exit then
  min-int 1 * min-int = 0= if 8 exit then
  0 ;

: test4
  \ max-int * -1 = -max-int = min-int + 1
  max-int -1 * max-int negate = 0= if 9 exit then
  0 ;

: test5
  \ MIN-INT * -1 overflows back to MIN-INT
  min-int -1 * min-int = 0= if 10 exit then
  0 ;

: test6
  \ Overflow: 2^32 * 2^32 = 0 (low 64 bits)
  1 32 lshift dup * 0= 0= if 11 exit then
  0 ;

: main
  test1 dup 0<> if exit then drop
  test2 dup 0<> if exit then drop
  test3 dup 0<> if exit then drop
  test4 dup 0<> if exit then drop
  test5 dup 0<> if exit then drop
  test6 ;

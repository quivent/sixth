\ expect: 0
\ Brutal Integration Test 07: Multiple array operations
\ Tests: several arrays, arithmetic combinations

variable arr-a
variable arr-b
variable arr-c

: init-arrs ( -- )
  here arr-a ! 4 cells allot
  here arr-b ! 4 cells allot
  here arr-c ! 4 cells allot
  \ Initialize arrays
  1 arr-a @ !
  2 arr-a @ cell+ !
  3 arr-b @ !
  4 arr-b @ cell+ ! ;

: compute ( -- )
  \ c[0] = a[0] * b[0]
  arr-a @ @ arr-b @ @ * arr-c @ !
  \ c[1] = a[1] * b[1]
  arr-a @ cell+ @ arr-b @ cell+ @ * arr-c @ cell+ ! ;

: main
  init-arrs
  compute
  \ c[0] should be 1*3=3
  arr-c @ @ 3 <> if 1 exit then
  \ c[1] should be 2*4=8
  arr-c @ cell+ @ 8 <> if 1 exit then
  0 ;

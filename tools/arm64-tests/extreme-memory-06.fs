\ expect: 0
\ Test: HERE manipulation with ALLOT and comma
\ Stress test dictionary pointer movement

variable saved-here

: test-comma ( -- flag )
  here saved-here !
  12345678 ,
  here saved-here @ - 8 <> if 0 exit then
  saved-here @ @ 12345678 <> if 0 exit then
  1 ;

: test-allot ( -- flag )
  here saved-here !
  100 allot
  here saved-here @ - 100 <> if 0 exit then
  \ Write to allocated space
  42 saved-here @ !
  saved-here @ @ 42 <> if 0 exit then
  1 ;

: test-c-comma ( -- flag )
  here saved-here !
  65 c,
  66 c,
  67 c,
  68 c,
  here saved-here @ - 4 <> if 0 exit then
  saved-here @ c@ 65 <> if 0 exit then
  saved-here @ 1+ c@ 66 <> if 0 exit then
  saved-here @ 2 + c@ 67 <> if 0 exit then
  saved-here @ 3 + c@ 68 <> if 0 exit then
  1 ;

: main
  test-comma 0= if 1 exit then
  test-allot 0= if 2 exit then
  test-c-comma 0= if 3 exit then
  0 ;

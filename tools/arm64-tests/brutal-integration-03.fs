\ expect: 0
\ Brutal Integration Test 03: Linear Search
\ Tests: loops, conditionals, memory, early exit

variable arr-base

: arr@ ( n -- val ) cells arr-base @ + @ ;
: arr! ( val n -- ) cells arr-base @ + ! ;

: init-arr ( -- )
  here arr-base !
  8 cells allot
  10 0 arr!  20 1 arr!  30 2 arr!  40 3 arr!
  50 4 arr!  60 5 arr!  70 6 arr!  80 7 arr! ;

: lsearch ( val -- idx|-1 )
  8 0 do
    dup i arr@ = if drop i unloop exit then
  loop
  drop -1 ;

: main
  init-arr
  10 lsearch 0 <> if 1 exit then
  80 lsearch 7 <> if 1 exit then
  40 lsearch 3 <> if 1 exit then
  5 lsearch -1 <> if 1 exit then
  100 lsearch -1 <> if 1 exit then
  0 ;

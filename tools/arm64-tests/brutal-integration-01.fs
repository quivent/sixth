\ expect: 0
\ Brutal Integration Test 01: Simple Array Sum and Max
\ Tests: memory operations, loops, conditionals, comparisons

variable arr-base
variable arr-size

: arr@ ( n -- val ) cells arr-base @ + @ ;
: arr! ( val n -- ) cells arr-base @ + ! ;

: init-array ( -- )
  here arr-base !
  5 cells allot
  5 arr-size !
  4 0 arr!  2 1 arr!  5 2 arr!  1 3 arr!  3 4 arr! ;

: arr-sum ( -- sum )
  0
  arr-size @ 0 do
    i arr@ +
  loop ;

: arr-max ( -- max )
  0 arr@
  arr-size @ 1 do
    i arr@ over > if drop i arr@ then
  loop ;

: arr-min ( -- min )
  0 arr@
  arr-size @ 1 do
    i arr@ over < if drop i arr@ then
  loop ;

: main
  init-array
  arr-sum 15 <> if 1 exit then
  arr-max 5 <> if 1 exit then
  arr-min 1 <> if 1 exit then
  0 ;

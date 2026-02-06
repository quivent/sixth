\ expect: 0
\ Test: Large values and boundary conditions
\ 64-bit values, max/min integers

variable bigval
variable fail-code

: main
  0 fail-code !

  \ Large positive value
  1000000000 bigval !
  bigval @ 1000000000 <> if 1 fail-code ! then

  \ Negative value
  fail-code @ 0= if
    -1000000000 bigval !
    bigval @ -1000000000 <> if 2 fail-code ! then
  then

  \ -1 (all bits set)
  fail-code @ 0= if
    -1 bigval !
    bigval @ -1 <> if 3 fail-code ! then
  then

  \ Zero
  fail-code @ 0= if
    0 bigval !
    bigval @ 0<> if 4 fail-code ! then
  then

  \ Test +! with large values
  fail-code @ 0= if
    0 bigval !
    1000000 bigval +!
    bigval @ 1000000 <> if 5 fail-code ! then
  then

  fail-code @ 0= if
    -500000 bigval +!
    bigval @ 500000 <> if 6 fail-code ! then
  then

  \ Overflow: incrementing should wrap
  fail-code @ 0= if
    -1 bigval !
    1 bigval +!
    bigval @ 0<> if 7 fail-code ! then
  then

  fail-code @ ;

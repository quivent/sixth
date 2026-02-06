\ expect: 0
\ Test: Basic variable @ and ! with multiple variables
\ Verify that storing to one variable doesn't corrupt another

variable x
variable y
variable z
variable fail-code

: main
  0 fail-code !

  42 x !
  99 y !
  -1 z !

  \ Check all values preserved
  x @ 42 <> if 1 fail-code ! then
  fail-code @ 0= if y @ 99 <> if 2 fail-code ! then then
  fail-code @ 0= if z @ -1 <> if 3 fail-code ! then then

  \ Modify middle variable
  1000 y !

  \ Check x and z unchanged
  fail-code @ 0= if x @ 42 <> if 4 fail-code ! then then
  fail-code @ 0= if z @ -1 <> if 5 fail-code ! then then
  fail-code @ 0= if y @ 1000 <> if 6 fail-code ! then then

  fail-code @ ;

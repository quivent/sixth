\ expect: -1 0 9223372036854775807 -9223372036854775808
\ Pick with extreme 64-bit values on the stack
\ Tests that pick doesn't corrupt large values during copy
: main
  -9223372036854775808   \ minimum signed 64-bit
  9223372036854775807    \ maximum signed 64-bit
  0                      \ zero
  -1                     \ all bits set
  \ Stack: min max 0 -1 (top)
  0 pick . cr   \ -1
  1 pick . cr   \ 0
  2 pick . cr   \ max
  3 pick . cr   \ min
  drop drop drop drop
;

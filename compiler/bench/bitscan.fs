\ expected: 9999985
\ Bit scan forward - find lowest set bit position

: bsf ( n -- pos )
  dup 0= if drop -1 exit then
  0 swap
  begin dup 1 and 0= while
    1 rshift swap 1+ swap
  repeat drop ;

: main
  0
  10000000 1 do
    i bsf +
  loop
  . cr ;

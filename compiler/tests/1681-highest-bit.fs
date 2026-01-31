\ expect: 7
\ Find position of highest set bit in 200
\ 200 = 11001000 -> bit 7 (0-indexed)
: highbit ( n -- pos )
  -1 swap
  begin dup 0> while
    1 rshift swap 1+ swap
  repeat drop ;
: main 200 highbit . cr ;

\ expect: 5
\ Count set bits in 237 (11101101 binary = 5+1+1+0+1+1+0+1 wait)
\ 237 = 128+64+32+8+4+1 = 11101101 -> 6 bits set
\ Let's use 55 = 00110111 -> 5 bits set
: popcount ( n -- count )
  0 swap
  begin dup 0> while
    dup 1 and rot + swap
    1 rshift
  repeat drop ;
: main 55 popcount . cr ;

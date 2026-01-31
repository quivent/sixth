\ expect: 8
\ How many bits in 255?
\ 255 = 11111111 => 8 bits
: bitcount ( n -- bits )
  0 swap
  begin dup 0 > while
    swap 1+ swap
    2 /
  repeat drop ;
: main 255 bitcount . cr ;

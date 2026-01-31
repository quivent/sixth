\ expect: 11
\ Reverse 4 bits of 13 (1101) -> 1011 = 11
: main
  13
  0 swap
  4 0 do
    swap 1 lshift over 1 and or swap
    1 rshift
  loop drop
  . cr ;

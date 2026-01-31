\ expect: 22
\ Convert binary 10110 to decimal
\ 1*16 + 0*8 + 1*4 + 1*2 + 0*1 = 22
create bits 40 allot
: bit@ ( i -- val ) 8 * bits + @ ;
: bit! ( val i -- ) 8 * bits + ! ;
: bin2dec ( n -- val )
  0 swap
  0 do
    2 * i bit@ +
  loop ;
: main
  1 0 bit!  0 1 bit!  1 2 bit!  1 3 bit!  0 4 bit!
  5 bin2dec . cr ;

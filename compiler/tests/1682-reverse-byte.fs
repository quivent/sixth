\ expect: 45
\ Reverse bits of a byte: 180 = 10110100 -> 00101101 = 45
: revbyte ( n -- reversed )
  0 swap                \ ( result input )
  8 0 do
    dup 1 and           \ ( result input bit )
    >r 1 rshift         \ ( result input>>1 ) R:(bit)
    swap 2* r> or swap  \ ( result<<1|bit input>>1 )
  loop drop ;
: main 180 revbyte . cr ;

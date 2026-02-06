\ Adversarial: Equal with various bit patterns
\ expect: 1
: MIN-INT 1 63 lshift ;

\ Alternating pattern: 10101010...
: ALT-A -6148914691236517206 ;   \ 0xAAAAAAAAAAAAAAAA
\ Alternating pattern: 01010101...
: ALT-5 6148914691236517205 ;    \ 0x5555555555555555

: main
  \ All zeros
  0 0 =
  -1 = 0= if 0 exit then

  \ All ones (-1 = -1)
  -1 -1 =
  -1 = 0= if 0 exit then

  \ Alternating bits
  ALT-A ALT-A =
  -1 = 0= if 0 exit then

  \ Alternating bits inverse
  ALT-5 ALT-5 =
  -1 = 0= if 0 exit then

  \ Different patterns must not be equal
  ALT-A ALT-5 =
  0= 0= if 0 exit then

  \ High bit only
  MIN-INT MIN-INT =
  -1 = 0= if 0 exit then

  \ Low bit only
  1 1 =
  -1 = 0= if 0 exit then

  \ Single bit difference
  -2 -1 =
  0= 0= if 0 exit then

  \ High bit difference
  0 MIN-INT =
  0= 0= if 0 exit then

  1 ;

\ Adversarial: Unsigned comparison with high bit set
\ Tests u< and u> where signed interpretation would give wrong result
\ expect: 1
: MIN-INT 1 63 lshift ;
: MAX-INT MIN-INT 1- ;
: MAX-UINT -1 ;

: main
  \ -1 as unsigned = MAX-UINT, so -1 u> 0 must be true
  -1 0 u>
  -1 = 0= if 0 exit then

  \ 1 u< -1 (unsigned: 1 < MAX-UINT) must be true
  1 -1 u<
  -1 = 0= if 0 exit then

  \ MIN-INT as unsigned = 0x8000..., so MIN-INT u> 0 must be true
  MIN-INT 0 u>
  -1 = 0= if 0 exit then

  \ MAX-INT u< MIN-INT (unsigned ordering) must be true
  MAX-INT MIN-INT u<
  -1 = 0= if 0 exit then

  \ Contrast with signed: MAX-INT > MIN-INT (signed) must be true
  MAX-INT MIN-INT >
  -1 = 0= if 0 exit then

  \ MAX-UINT u> MAX-UINT-1 must be true
  MAX-UINT MAX-UINT 1- u>
  -1 = 0= if 0 exit then

  \ 0 u< 1 must be true
  0 1 u<
  -1 = 0= if 0 exit then

  \ Equal values: -1 u< -1 must be false
  -1 -1 u<
  0= 0= if 0 exit then

  1 ;

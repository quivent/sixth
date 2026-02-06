\ Adversarial: 0> edge cases (is TOS positive?)
\ expect: 1
: MIN-INT 1 63 lshift ;
: MAX-INT MIN-INT 1- ;

: main
  \ 1 0> must be true (positive)
  1 0>
  -1 = 0= if 0 exit then

  \ 0 0> must be false (zero is not positive)
  0 0>
  0= 0= if 0 exit then

  \ -1 0> must be false (negative)
  -1 0>
  0= 0= if 0 exit then

  \ MAX-INT 0> must be true
  MAX-INT 0>
  -1 = 0= if 0 exit then

  \ MIN-INT 0> must be false
  MIN-INT 0>
  0= 0= if 0 exit then

  \ MAX-INT 0> must be true
  MAX-INT 0>
  -1 = 0= if 0 exit then

  \ MIN-INT 0> must be false (it's negative when signed)
  MIN-INT 0>
  0= 0= if 0 exit then

  1 ;

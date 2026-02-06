\ Adversarial: 0= edge cases
\ expect: 1
: MIN-INT 1 63 lshift ;
: MAX-INT MIN-INT 1- ;

: main
  \ 0 0= must be true (-1)
  0 0=
  -1 = 0= if 0 exit then

  \ 1 0= must be false (0)
  1 0=
  0= 0= if 0 exit then

  \ -1 0= must be false
  -1 0=
  0= 0= if 0 exit then

  \ MIN-INT 0= must be false
  MIN-INT 0=
  0= 0= if 0 exit then

  \ MAX-INT 0= must be false
  MAX-INT 0=
  0= 0= if 0 exit then

  \ 0<> inverse: 0 0<> must be false
  0 0<>
  0= 0= if 0 exit then

  \ 1 0<> must be true
  1 0<>
  -1 = 0= if 0 exit then

  \ -1 0= must be false (it's -1, not 0)
  -1 0=
  0= 0= if 0 exit then

  1 ;

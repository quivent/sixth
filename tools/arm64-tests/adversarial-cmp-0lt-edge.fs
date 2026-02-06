\ Adversarial: 0< edge cases (is TOS negative?)
\ expect: 1
: MIN-INT 1 63 lshift ;
: MAX-INT MIN-INT 1- ;

: main
  \ -1 0< must be true (negative)
  -1 0<
  -1 = 0= if 0 exit then

  \ 0 0< must be false (zero is not negative)
  0 0<
  0= 0= if 0 exit then

  \ 1 0< must be false (positive)
  1 0<
  0= 0= if 0 exit then

  \ MIN-INT 0< must be true (most negative number)
  MIN-INT 0<
  -1 = 0= if 0 exit then

  \ MAX-INT 0< must be false (most positive number)
  MAX-INT 0<
  0= 0= if 0 exit then

  \ MIN-INT 0< must be true (it's MIN-INT when interpreted signed)
  MIN-INT 0<
  -1 = 0= if 0 exit then

  \ MAX-INT 0< must be false
  MAX-INT 0<
  0= 0= if 0 exit then

  1 ;

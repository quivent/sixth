\ Adversarial: MIN-INT signed comparison edge cases
\ MIN-INT = -9223372036854775808 = 0x8000000000000000
\ expect: 1
: MIN-INT 1 63 lshift ;
: MAX-INT MIN-INT 1- ;

: main
  \ MIN-INT < 0 must be true
  MIN-INT 0 <
  -1 = 0= if 0 exit then

  \ MIN-INT < -1 must be true (most negative < less negative)
  MIN-INT -1 <
  -1 = 0= if 0 exit then

  \ MIN-INT < MAX-INT must be true
  MIN-INT MAX-INT <
  -1 = 0= if 0 exit then

  \ MIN-INT > anything-positive must be false
  MIN-INT 1 >
  0= 0= if 0 exit then

  \ MIN-INT = MIN-INT must be true
  MIN-INT MIN-INT =
  -1 = 0= if 0 exit then

  \ MIN-INT <> MAX-INT must be true
  MIN-INT MAX-INT <>
  -1 = 0= if 0 exit then

  \ MIN-INT >= MIN-INT must be true
  MIN-INT MIN-INT >=
  -1 = 0= if 0 exit then

  \ MIN-INT <= MIN-INT must be true
  MIN-INT MIN-INT <=
  -1 = 0= if 0 exit then

  1 ;

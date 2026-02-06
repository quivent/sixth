\ Adversarial: MAX-INT signed comparison edge cases
\ MAX-INT = 9223372036854775807 = 0x7FFFFFFFFFFFFFFF
\ expect: 1
: MIN-INT 1 63 lshift ;
: MAX-INT MIN-INT 1- ;

: main
  \ MAX-INT > 0 must be true
  MAX-INT 0 >
  -1 = 0= if 0 exit then

  \ MAX-INT > -1 must be true
  MAX-INT -1 >
  -1 = 0= if 0 exit then

  \ MAX-INT > MIN-INT must be true
  MAX-INT MIN-INT >
  -1 = 0= if 0 exit then

  \ MAX-INT < anything must be false (except itself)
  MAX-INT 1 <
  0= 0= if 0 exit then

  \ MAX-INT = MAX-INT must be true
  MAX-INT MAX-INT =
  -1 = 0= if 0 exit then

  \ MAX-INT >= 0 must be true
  MAX-INT 0 >=
  -1 = 0= if 0 exit then

  \ MAX-INT <= MAX-INT must be true
  MAX-INT MAX-INT <=
  -1 = 0= if 0 exit then

  \ Comparing MAX-INT with MAX-INT-1
  MAX-INT MAX-INT 1- >
  -1 = 0= if 0 exit then

  1 ;

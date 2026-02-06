\ Adversarial: min/max operations
\ Implement min/max manually and test edge cases
\ expect: 1
: MIN-INT 1 63 lshift ;
: MAX-INT MIN-INT 1- ;

: my-min ( a b -- min )
  2dup > if swap then drop ;

: my-max ( a b -- max )
  2dup < if swap then drop ;

: main
  \ Basic min
  3 5 my-min
  3 = 0= if 0 exit then

  \ Basic max
  3 5 my-max
  5 = 0= if 0 exit then

  \ Equal values
  7 7 my-min
  7 = 0= if 0 exit then

  7 7 my-max
  7 = 0= if 0 exit then

  \ Negative numbers
  -5 -3 my-min
  -5 = 0= if 0 exit then

  -5 -3 my-max
  -3 = 0= if 0 exit then

  \ Mixed signs
  -5 3 my-min
  -5 = 0= if 0 exit then

  -5 3 my-max
  3 = 0= if 0 exit then

  \ Extreme values
  MIN-INT MAX-INT my-min
  MIN-INT = 0= if 0 exit then

  MIN-INT MAX-INT my-max
  MAX-INT = 0= if 0 exit then

  \ MIN-INT with 0
  MIN-INT 0 my-min
  MIN-INT = 0= if 0 exit then

  \ MAX-INT with 0
  MAX-INT 0 my-max
  MAX-INT = 0= if 0 exit then

  1 ;

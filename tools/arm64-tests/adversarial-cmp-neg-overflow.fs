\ Adversarial: Negative number comparison with potential overflow
\ Test cases where subtraction-based comparison could overflow
\ expect: 1
: MIN-INT 1 63 lshift ;
: MAX-INT MIN-INT 1- ;

: main
  \ MIN-INT - 1 would overflow, but < should still work correctly
  \ MIN-INT < 1 must be true (signed comparison)
  MIN-INT 1 <
  -1 = 0= if 0 exit then

  \ MAX-INT - (-1) would overflow, but > should still work
  \ MAX-INT > -1 must be true
  MAX-INT -1 >
  -1 = 0= if 0 exit then

  \ Comparing two large negatives
  \ -1 > MIN-INT must be true
  -1 MIN-INT >
  -1 = 0= if 0 exit then

  \ MIN-INT < MIN-INT+1 must be true
  MIN-INT MIN-INT 1+ <
  -1 = 0= if 0 exit then

  \ MAX-INT-1 < MAX-INT must be true
  MAX-INT 1- MAX-INT <
  -1 = 0= if 0 exit then

  \ -2 < -1 must be true
  -2 -1 <
  -1 = 0= if 0 exit then

  \ -1 > -2 must be true
  -1 -2 >
  -1 = 0= if 0 exit then

  1 ;

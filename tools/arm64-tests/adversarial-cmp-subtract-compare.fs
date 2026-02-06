\ Adversarial: Comparison via subtraction patterns
\ Tests that comparison doesn't confuse sign bit with overflow
\ expect: 1
: MIN-INT 1 63 lshift ;
: MAX-INT MIN-INT 1- ;

: main
  \ Subtraction that wraps: 0 - MIN-INT = MIN-INT (overflow)
  \ But 0 > MIN-INT should be true (0 is greater than most negative)
  0 MIN-INT >
  -1 = 0= if 0 exit then

  \ MAX-INT - (-1) overflows, but MAX-INT > -1 should be true
  MAX-INT -1 >
  -1 = 0= if 0 exit then

  \ Two negatives: -1 - MIN-INT would overflow
  \ But -1 > MIN-INT should be true
  -1 MIN-INT >
  -1 = 0= if 0 exit then

  \ Large positive - large positive (no overflow, straightforward)
  MAX-INT MAX-INT 1- >
  -1 = 0= if 0 exit then

  \ Check that actual subtraction gives expected (possibly wrapped) result
  \ but comparison still works correctly
  \ 1 - MAX-INT = 1 - 0x7FFF... = negative (wraps)
  \ But 1 < MAX-INT should be true
  1 MAX-INT <
  -1 = 0= if 0 exit then

  \ Test the difference between subtraction and comparison
  \ MIN-INT - 1 wraps to MAX-INT
  \ But MIN-INT < 1 must still be true
  MIN-INT 1 <
  -1 = 0= if 0 exit then

  1 ;

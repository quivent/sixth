\ expect: 0
\ Adversarial: 2swap with extreme 64-bit values
\ Tests: 0, -1, MAX_INT64, MIN_INT64
\ Returns 0 if all swaps correct
: main
  0 -1 9223372036854775807 -9223372036854775808
  \ Stack: ( 0 -1 MAX MIN )
  2swap
  \ Stack should be: ( MAX MIN 0 -1 )
  -1 - abs              \ TOS should be -1
  swap 0 - abs +        \ second should be 0
  swap -9223372036854775808 - abs +  \ third should be MIN
  swap 9223372036854775807 - abs +   \ fourth should be MAX
;

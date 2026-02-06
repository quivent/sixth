\ expect: 0
\ Test: Multiple variables with interleaved access
\ Stress test variable addressing

variable a variable b variable c variable d
variable e variable f variable g variable h

: init-vars ( -- )
  1 a ! 2 b ! 3 c ! 4 d !
  5 e ! 6 f ! 7 g ! 8 h ! ;

: sum-vars ( -- n )
  a @ b @ + c @ + d @ +
  e @ + f @ + g @ + h @ + ;

: swap-vars ( -- )
  a @ h @ a ! h !
  b @ g @ b ! g !
  c @ f @ c ! f !
  d @ e @ d ! e ! ;

: verify-swap ( -- flag )
  a @ 8 <> if 0 exit then
  b @ 7 <> if 0 exit then
  c @ 6 <> if 0 exit then
  d @ 5 <> if 0 exit then
  e @ 4 <> if 0 exit then
  f @ 3 <> if 0 exit then
  g @ 2 <> if 0 exit then
  h @ 1 <> if 0 exit then
  1 ;

: main
  init-vars
  sum-vars 36 <> if 1 exit then
  swap-vars
  sum-vars 36 <> if 2 exit then
  verify-swap 0= if 3 exit then
  0 ;

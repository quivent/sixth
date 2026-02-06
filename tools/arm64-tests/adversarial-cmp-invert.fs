\ Adversarial: Test invert with 0>= and 0<= results
\ expect: 1
: main
  \ 0>= returns -1 (all bits set) when true
  \ invert of -1 is 0
  5 0>=            \ true (-1)
  invert           \ 0
  0= if
    \ 0<= returns -1 when true
    \ invert of -1 is 0
    -5 0<=         \ true (-1)
    invert         \ 0
    0= if 1 else 0 then
  else
    0
  then ;

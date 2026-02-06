\ Adversarial: Verify exact return values are -1 and 0
\ expect: 1
: main
  \ 0>= should return exactly -1, not just non-zero
  5 0>= -1 - 0= if    \ result - (-1) should be 0
    \ 0<= should return exactly 0 for false case
    5 0<= 0 - 0= if   \ result - 0 should be 0
      \ 0>= should return exactly 0 for false case
      -5 0>= 0 - 0= if
        \ 0<= should return exactly -1
        -5 0<= -1 - 0= if 1 else 0 then
      else 0 then
    else 0 then
  else 0 then ;

\ expect: 0
\ Test: Zero-comparison words (0=, 0<>, 0<, 0>)

: main
  \ 0= tests
  0 0= -1 <> if 1 exit then                 \ 0 0= must be true
  1 0= 0 <> if 2 exit then                  \ 1 0= must be false
  -1 0= 0 <> if 3 exit then                 \ -1 0= must be false

  \ 0<> tests
  0 0<> 0 <> if 4 exit then                 \ 0 0<> must be false
  1 0<> -1 <> if 5 exit then                \ 1 0<> must be true
  -1 0<> -1 <> if 6 exit then               \ -1 0<> must be true

  \ 0< tests (signed less than zero)
  -1 0< -1 <> if 7 exit then                \ -1 0< must be true
  0 0< 0 <> if 8 exit then                  \ 0 0< must be false
  1 0< 0 <> if 9 exit then                  \ 1 0< must be false

  \ 0> tests (signed greater than zero)
  1 0> -1 <> if 10 exit then                \ 1 0> must be true
  0 0> 0 <> if 11 exit then                 \ 0 0> must be false
  -1 0> 0 <> if 12 exit then                \ -1 0> must be false

  0
;

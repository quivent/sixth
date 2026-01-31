\ expect: 10 50 30
\ Clamp value to range [10, 50]
: clamp ( val lo hi -- clamped ) rot min max ;
: main
  5 10 50 clamp . cr
  75 10 50 clamp . cr
  30 10 50 clamp . cr ;

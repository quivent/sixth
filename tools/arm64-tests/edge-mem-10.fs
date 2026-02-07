\ expect: 210
\ EDGE: Negative values in memory (signed representation)
\ Tests: Store and retrieve negative numbers, verify 2's complement

create cell1 8 allot

: main
  \ Store a negative number
  -100 cell1 !

  \ Read it back and verify it's still negative
  cell1 @         \ Should be -100

  \ Add a positive offset to bring into valid exit code range
  \ -100 + 255 = 155
  255 +

  \ Store another negative
  -50 cell1 !

  \ Combine: previous result + negated cell value
  \ 155 + 50 = 205
  cell1 @ negate +

  \ Add small adjustment
  5 +             \ 205 + 5 = 210
;

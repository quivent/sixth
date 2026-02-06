\ expect: 0
\ Test: 2DROP correctness - must remove exactly 2 items
\ (a b c d 2drop) = (a b)

: main
  10 20 30 40
  2drop         ( Stack: 10 20 )
  20 - swap 10 - or
;

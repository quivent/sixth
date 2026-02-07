\ expect: 77
\ EDGE: Stress test +! (add-to-memory) operation
\ Tests: Multiple +! operations to same cell, verify final value

create counter 8 allot

: main
  0 counter !       \ Initialize to 0

  \ Add various values using +!
  10 counter +!     \ 0 + 10 = 10
  20 counter +!     \ 10 + 20 = 30
  -5 counter +!     \ 30 - 5 = 25  (test negative add)
  50 counter +!     \ 25 + 50 = 75
  2 counter +!      \ 75 + 2 = 77

  counter @         \ Should be 77
;

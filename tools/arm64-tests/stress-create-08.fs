\ expect: 42
\ STRESS: Many small adjacent buffers
\ Tests: 8 buffers of 8 bytes each, verify independence

create b0 8 allot
create b1 8 allot
create b2 8 allot
create b3 8 allot
create b4 8 allot
create b5 8 allot
create b6 8 allot
create b7 8 allot

: main
  42 b0 !
  1 b1 !
  2 b2 !
  3 b3 !
  4 b4 !
  5 b5 !
  6 b6 !
  7 b7 !
  \ Verify b0 wasn't corrupted by any of the other stores
  b0 @
;

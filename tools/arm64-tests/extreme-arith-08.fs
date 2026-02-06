\ expect: 0
\ MIN_INT64 / -1 overflow case
\ Result should be MIN_INT64 (negative)
\ Tests signed division overflow handling
: main 1 63 lshift -1 / 0< if 0 else 1 then ;

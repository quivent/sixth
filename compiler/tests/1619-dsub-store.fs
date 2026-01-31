\ expect: 0 50
\ d- ( lo1 hi1 lo2 hi2 -- lo hi )
\ 100 0 - 50 0 = 50 0
\ print hi then lo
: main
  100 0 50 0 d-
  . . cr ;

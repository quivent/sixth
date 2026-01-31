\ expect: 0 30
\ d+ ( lo1 hi1 lo2 hi2 -- lo hi )
\ 10 0 + 20 0 = 30 0
\ print hi then lo
variable lo
variable hi
: main
  10 0 20 0 d+
  hi ! lo !
  hi @ . lo @ . cr ;

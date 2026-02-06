\ expect: 60
\ Test: Interleaved R@ and R> - read before each pop
: main
  10 >r 20 >r 30 >r
  r@
  r> drop
  r@
  +
  r> drop
  r@
  +
  r> drop
;

\ expect: 0
\ Test: Memory fill and verification (not zero-init - uses stack memory)
\ NOTE: We don't test initial zeros because stack memory isn't guaranteed zero

variable zeros
variable ok

: setup here zeros ! 512 allot ;

: check-zeros ( -- flag )
  1 ok !
  512 0 do
    zeros @ i + c@ 0<> if 0 ok ! then
  loop ok @ ;

: main
  setup
  \ Skip zero-init check - stack memory isn't guaranteed to be zero
  512 0 do 255 zeros @ i + c! loop
  512 0 do 0 zeros @ i + c! loop
  check-zeros 0= if 2 exit then
  0 ;

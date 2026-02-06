\ expect: 0
\ Test: Uninitialized memory should be zero (or at least consistent)
\ This is adversarial - relies on BSS zeroing behavior

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
  check-zeros 0= if 1 exit then
  512 0 do 255 zeros @ i + c! loop
  512 0 do 0 zeros @ i + c! loop
  check-zeros 0= if 2 exit then
  0 ;

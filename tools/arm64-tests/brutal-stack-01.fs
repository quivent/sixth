\ expect: 0
\ Test: DUP chain followed by arithmetic - stresses TOS register caching
\ If TOS is cached in register, chain of DUPs must properly spill to memory

: main
  42 dup dup dup dup dup    ( 6 copies of 42 on stack )
  + + + + +                  ( sum = 42*6 = 252 )
  252 - ( should be 0 )
;

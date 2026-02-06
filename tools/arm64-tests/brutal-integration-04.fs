\ expect: 0
\ Brutal Integration Test 04: GCD and LCM
\ Tests: recursion, arithmetic, modulo, register pressure

: gcd ( a b -- gcd )
  dup 0= if drop exit then
  swap over mod gcd ;

: lcm ( a b -- lcm )
  2dup gcd
  rot over / rot * ;

: coprime? ( a b -- flag )
  gcd 1 = ;

: main
  \ Test GCD
  48 18 gcd 6 <> if 1 exit then
  100 35 gcd 5 <> if 1 exit then
  17 13 gcd 1 <> if 1 exit then
  0 5 gcd 5 <> if 1 exit then
  \ Test LCM
  4 6 lcm 12 <> if 1 exit then
  3 5 lcm 15 <> if 1 exit then
  7 11 lcm 77 <> if 1 exit then
  \ Test coprime
  8 15 coprime? 0= if 1 exit then
  8 12 coprime? if 1 exit then
  0 ;

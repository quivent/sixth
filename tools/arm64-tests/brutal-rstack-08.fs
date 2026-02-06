\ Brutal Return Stack Test 08: Return stack with BEGIN-UNTIL loops

: t-survives
  100 >r
  0 begin 1 + dup 5 = until drop
  r> 100 = if ." PASS" else ." FAIL" then cr ;

\ Simpler inside test - count using r@ in loop
: t-inside
  5 >r
  0 begin r@ + r> 1 - dup >r 0= until
  r> drop
  15 = if ." PASS" else ." FAIL" then cr ;

: main
  ." brutal-rstack-08: Return Stack with BEGIN-UNTIL" cr
  ." survives: " t-survives
  ." inside:   " t-inside
  0 ;

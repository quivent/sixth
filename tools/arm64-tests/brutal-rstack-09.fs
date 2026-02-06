\ Brutal Return Stack Test 09: Return stack with BEGIN-WHILE-REPEAT

: t-survives
  88 >r
  5 begin dup 0> while 1 - repeat drop
  r> 88 = if ." PASS" else ." FAIL" then cr ;

: t-accum
  100 >r
  0 5 begin dup 0> while dup rot + swap 1 - repeat drop
  r> drop
  15 = if ." PASS" else ." FAIL" then cr ;

: main
  ." brutal-rstack-09: Return Stack with BEGIN-WHILE-REPEAT" cr
  ." survives:   " t-survives
  ." accumulate: " t-accum
  0 ;

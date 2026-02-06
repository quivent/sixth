\ Brutal Return Stack Test 05: Return stack with DO-LOOP (loop uses rstack!)

: test-rstack-survives-loop
  45 >r
  5 0 do loop
  r> 45 = if ." PASS" else ." FAIL" then cr ;

: test-rstack-survives-counted-loop
  100 >r
  3 0 do loop
  r> 100 = if ." PASS" else ." FAIL" then cr ;

: test-nested-loops-with-rstack
  77 >r
  2 0 do 2 0 do loop loop
  r> 77 = if ." PASS" else ." FAIL" then cr ;

: main
  ." brutal-rstack-05: Return Stack with DO-LOOP" cr
  ." survives:     " test-rstack-survives-loop
  ." counted:      " test-rstack-survives-counted-loop
  ." nested-loops: " test-nested-loops-with-rstack
  0 ;

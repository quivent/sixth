\ Brutal Return Stack Test 06: Deep return stack usage (8 levels)

: t-verify
  1 >r 2 >r 3 >r 4 >r 5 >r 6 >r 7 >r 8 >r
  r> r> r> r> r> r> r> r>
  1 = >r 2 = >r 3 = >r 4 = >r 5 = >r 6 = >r 7 = >r 8 =
  r> and r> and r> and r> and r> and r> and r> and
  if ." PASS" else ." FAIL" then cr ;

: t-compute
  1 >r 2 >r 3 >r 4 >r 5 >r 6 >r 7 >r 8 >r
  r> r> r> r> r> r> r> r>
  + + + + + + +
  36 = if ." PASS" else ." FAIL" then cr ;

: main
  ." brutal-rstack-06: Deep Return Stack (8 levels)" cr
  ." verify-all: " t-verify
  ." compute:    " t-compute
  0 ;

\ Brutal Return Stack Test 03: Multiple values - LIFO order

: t-lifo2
  11 >r 22 >r
  r> 22 = swap
  r> 11 = and
  if ." PASS" else ." FAIL" then cr ;

: t-lifo3
  1 >r 2 >r 3 >r
  r> 3 = rot rot
  r> 2 = rot rot and
  r> 1 = and
  if ." PASS" else ." FAIL" then cr ;

: t-lifo4
  10 >r 20 >r 30 >r 40 >r
  r> 40 = >r
  r> 30 = >r
  r> 20 = >r
  r> 10 =
  r> r> r> and and and
  if ." PASS" else ." FAIL" then cr ;

: main
  ." brutal-rstack-03: LIFO Order Verification" cr
  ." lifo-2: " t-lifo2
  ." lifo-3: " t-lifo3
  ." lifo-4: " t-lifo4
  0 ;

\ Brutal Return Stack Test 01: Basic >R R> round-trip preserves value

: t-basic
  42 >r r> 42 = if ." PASS" else ." FAIL" then cr ;

: t-zero
  0 >r r> 0 = if ." PASS" else ." FAIL" then cr ;

: t-neg
  -1 >r r> -1 = if ." PASS" else ." FAIL" then cr ;

: t-large
  2147483647 >r r> 2147483647 = if ." PASS" else ." FAIL" then cr ;

: main
  ." brutal-rstack-01: Basic >R R> Round-trip" cr
  ." basic:    " t-basic
  ." zero:     " t-zero
  ." negative: " t-neg
  ." large:    " t-large
  0 ;

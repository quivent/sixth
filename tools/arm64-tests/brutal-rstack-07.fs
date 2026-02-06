\ Brutal Return Stack Test 07: Return stack with IF-THEN control flow

: t-if-true
  33 >r
  1 if r@ else 0 then
  33 = if ." PASS" else ." FAIL" then r> drop cr ;

: t-if-false
  44 >r
  0 if 0 else r@ then
  44 = if ." PASS" else ." FAIL" then r> drop cr ;

: t-nested
  55 >r
  1 if 1 if r@ else 0 then else 0 then
  55 = if ." PASS" else ." FAIL" then r> drop cr ;

: main
  ." brutal-rstack-07: Return Stack with IF-THEN" cr
  ." if-true:  " t-if-true
  ." if-false: " t-if-false
  ." nested:   " t-nested
  0 ;

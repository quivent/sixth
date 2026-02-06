\ Brutal Return Stack Test 02: R@ reads without consuming

: t-preserves
  10 >r r@ r> = if ." PASS" else ." FAIL" then cr ;

: t-multi
  77 >r r@ r@ = if ." PASS" else ." FAIL" then r> drop cr ;

: t-correct
  99 >r r@ 99 = if ." PASS" else ." FAIL" then r> drop cr ;

: main
  ." brutal-rstack-02: R@ Non-Consuming Read" cr
  ." preserves:  " t-preserves
  ." multi-read: " t-multi
  ." correct:    " t-correct
  0 ;

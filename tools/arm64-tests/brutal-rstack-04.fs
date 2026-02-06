\ Brutal Return Stack Test 04: Return stack across word calls

: inner ( -- n ) r@ ;

: t-across
  99 >r inner 99 = if ." PASS" else ." FAIL" then r> drop cr ;

: dbl-inner ( -- n ) r@ r@ + ;

: t-double
  50 >r dbl-inner 100 = if ." PASS" else ." FAIL" then r> drop cr ;

: nested ( -- n ) inner ;

: t-deep
  123 >r nested 123 = if ." PASS" else ." FAIL" then r> drop cr ;

: main
  ." brutal-rstack-04: Return Stack Across Word Calls" cr
  ." basic-call:  " t-across
  ." double-read: " t-double
  ." deep-calls:  " t-deep
  0 ;

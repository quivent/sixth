\ Test 1022: Constant folding - drop optimization
\ REGRESSION: When drop follows a literal on the ct-stack, the compiler
\ should discard the constant without emitting any code (ct-pop + discard).
: val ( -- n ) 42 ;
: main val 99 drop 42 = 0= if begin again then ;

\ Test 1009: Constant folding - unary 1+
\ REGRESSION: Verifies compile-time folding of 1+ on a literal
: fail begin again ;
: main 99 1+ 100 = 0= if fail then ;

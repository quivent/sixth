\ Test 1010: Constant folding - unary 1-
\ REGRESSION: Verifies compile-time folding of 1- on a literal
: fail begin again ;
: main 100 1- 99 = 0= if fail then ;

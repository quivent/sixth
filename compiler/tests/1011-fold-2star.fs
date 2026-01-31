\ Test 1011: Constant folding - unary 2*
\ REGRESSION: Verifies compile-time folding of 2* on a literal
: fail begin again ;
: main 21 2* 42 = 0= if fail then ;

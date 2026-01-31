\ Test 1013: Constant folding - unary invert
\ REGRESSION: Verifies compile-time folding of invert on a literal
: fail begin again ;
: main 0 invert -1 = 0= if fail then ;

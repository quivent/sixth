\ expect:
\ Test 1008: Constant folding - unary negate
\ REGRESSION: Verifies compile-time folding of negate on a literal
: fail begin again ;
: main 5 negate -5 = 0= if fail then ;

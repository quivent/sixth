\ Test 1044: Constant folding - 2+
\ REGRESSION: Verifies compile-time folding of 2+ on a literal.
: fail begin again ;
: main 40 2+ 42 = 0= if fail then ;

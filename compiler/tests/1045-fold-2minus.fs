\ Test 1045: Constant folding - 2-
\ REGRESSION: Verifies compile-time folding of 2- on a literal.
: fail begin again ;
: main 44 2- 42 = 0= if fail then ;

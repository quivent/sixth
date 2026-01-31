\ Test 1014: Constant folding - unary abs
\ REGRESSION: Verifies compile-time folding of abs on a literal
: fail begin again ;
: main -42 abs 42 = 0= if fail then ;

\ Test 1004: Constant folding - modulo
\ REGRESSION: Verifies compile-time constant folding for mod
: fail begin again ;
: main 17 5 mod 2 = 0= if fail then ;

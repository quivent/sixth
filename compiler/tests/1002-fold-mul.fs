\ expect:
\ Test 1002: Constant folding - multiplication
\ REGRESSION: Verifies compile-time constant folding for *
: fail begin again ;
: main 7 6 * 42 = 0= if fail then ;

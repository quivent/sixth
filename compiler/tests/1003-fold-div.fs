\ expect:
\ Test 1003: Constant folding - division
\ REGRESSION: Verifies compile-time constant folding for /
: fail begin again ;
: main 20 4 / 5 = 0= if fail then ;

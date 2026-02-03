\ expect:
\ Test 1001: Constant folding - subtraction
\ REGRESSION: Verifies compile-time constant folding for -
: fail begin again ;
: main 10 3 - 7 = 0= if fail then ;

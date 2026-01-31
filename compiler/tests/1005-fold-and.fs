\ Test 1005: Constant folding - bitwise AND
\ REGRESSION: Verifies compile-time constant folding for and
: fail begin again ;
: main $FF0F $0F0F and $0F0F = 0= if fail then ;

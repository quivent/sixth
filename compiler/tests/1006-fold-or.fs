\ expect:
\ Test 1006: Constant folding - bitwise OR
\ REGRESSION: Verifies compile-time constant folding for or
: fail begin again ;
: main $F000 $00FF or $F0FF = 0= if fail then ;

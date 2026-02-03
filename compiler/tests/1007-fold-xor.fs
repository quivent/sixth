\ expect:
\ Test 1007: Constant folding - bitwise XOR
\ REGRESSION: Verifies compile-time constant folding for xor
: fail begin again ;
: main $FFFF $FF00 xor $00FF = 0= if fail then ;

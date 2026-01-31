\ Test 1027: Constant folding with hex constants
\ REGRESSION: Hex literals ($FF, $1000) must enter ct-stack correctly.
: fail begin again ;
: main $FF $100 + $1FF = 0= if fail then ;

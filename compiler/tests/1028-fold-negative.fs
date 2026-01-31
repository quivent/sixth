\ Test 1028: Constant folding with negative results
\ REGRESSION: Folded negative values must be handled correctly.
: fail begin again ;
: main 3 10 - -7 = 0= if fail then ;

\ Test 1029: Constant folding with zero
\ REGRESSION: Zero as folded result must work (edge case for some ops).
: fail begin again ;
: main 5 5 - 0 = 0= if fail then ;

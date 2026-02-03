\ expect:
\ Test 1015: Constant folding - chained operations
\ REGRESSION: Verifies that multiple constant folds chain correctly.
\ 3 * 7 + $FFFFFF and should fully fold at compile time when all
\ operands are literals: (((2 * 3) + 7) & $FFFFFF) = 13
: fail begin again ;
: main 2 3 * 7 + $FFFFFF and 13 = 0= if fail then ;

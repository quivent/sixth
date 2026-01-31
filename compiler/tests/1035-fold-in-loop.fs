\ Test 1035: Constant folding inside a loop
\ REGRESSION: Folding must work correctly inside begin/while/repeat loops.
\ ct-flush must happen at control flow boundaries.
: main 0 10 begin dup while swap 2 3 * + swap 1- repeat drop 60 = 0= if begin again then ;

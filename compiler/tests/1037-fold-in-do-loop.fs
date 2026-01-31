\ Test 1037: Constant folding inside do loop
\ REGRESSION: Folding constants inside do/loop must work correctly.
: main 0 5 0 do 2 3 * + loop 30 = 0= if begin again then ;

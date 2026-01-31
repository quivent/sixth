\ Test 1042: Deep constant folding chain (8 operations)
\ REGRESSION: The ct-stack is 8 cells deep. This chains 8 folding ops
\ to verify the full stack depth works.
: fail begin again ;
: main 1 2 + 3 + 4 + 5 + 6 + 7 + 8 + 36 = 0= if fail then ;

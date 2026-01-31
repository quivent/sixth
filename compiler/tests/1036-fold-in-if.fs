\ Test 1036: Constant folding inside if/then
\ REGRESSION: ct-flush at if/else/then boundaries must preserve correctness.
: main 1 if 3 4 + else 0 then 7 = 0= if begin again then ;

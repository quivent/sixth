\ Test 1025: ct-flush before swap
\ REGRESSION: swap must flush ct-stack first so pending constants
\ are materialized before the swap operates.
: five ( -- 5 ) 5 ;
: main five 10 swap - 5 = 0= if begin again then ;

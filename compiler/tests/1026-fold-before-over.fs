\ Test 1026: ct-flush before over
\ REGRESSION: over must flush ct-stack first.
: three ( -- 3 ) 3 ;
: main three 10 over + + 16 = 0= if begin again then ;

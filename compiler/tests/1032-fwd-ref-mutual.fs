\ expect:
\ Test 1032: Double pass - mutual forward reference pattern
\ REGRESSION: A calls B (forward), B calls C (forward). Pass 1 must
\ discover all three before Pass 2 compiles.
: main 5 step-a 12 = 0= if begin again then ;
: step-a ( n -- n ) step-b 2 + ;
: step-b ( n -- n ) 2* ;

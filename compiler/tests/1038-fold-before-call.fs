\ Test 1038: ct-flush before user word call
\ REGRESSION: When calling a user-defined word, pending constants on
\ the ct-stack must be flushed first so they're available as arguments.
: add3 ( a b c -- sum ) + + ;
: main 10 20 30 add3 60 = 0= if begin again then ;

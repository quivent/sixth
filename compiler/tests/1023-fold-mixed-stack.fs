\ Test 1023: Constant folding with runtime values on stack
\ REGRESSION: Ensures ct-flush correctly emits pending constants
\ before stack operations that mix runtime and compile-time values.
: seven ( -- 7 ) 7 ;
: main seven 3 + 10 = 0= if begin again then ;

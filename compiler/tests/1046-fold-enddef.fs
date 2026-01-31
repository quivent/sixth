\ Test 1046: ct-flush at end of definition
\ REGRESSION: A literal as the last thing before ; must be flushed
\ so it's returned as the function result.
: forty-two ( -- 42 ) 6 7 * ;
: main forty-two 42 = 0= if begin again then ;

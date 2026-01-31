\ expect: 42
\ Test 1039: Folded constant passed as argument to a word
\ REGRESSION: A folded constant (6*7=42) must be flushed correctly
\ when passed as an argument to a user-defined word.
: check ( n -- n ) dup 42 = 0= if begin again then ;
: main 6 7 * check . cr ;

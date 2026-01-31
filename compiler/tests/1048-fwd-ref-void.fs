\ expect: 42
\ Test 1048: Double pass - forward reference to word with side effects
\ REGRESSION: Forward-referenced word that uses I/O. Pass 1 must detect
\ has-io flag. The word prints but the test validates it compiles+runs.
: main 42 show-val ;
: show-val ( n -- ) . cr ;

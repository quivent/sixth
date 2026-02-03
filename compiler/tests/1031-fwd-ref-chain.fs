\ expect:
\ Test 1031: Double pass - chained forward references
\ REGRESSION: Multiple forward references resolved via info table.
: main 10 triple double 60 = 0= if begin again then ;
: double ( n -- n*2 ) 2* ;
: triple ( n -- n*3 ) dup 2* + ;

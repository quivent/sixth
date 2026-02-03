\ expect:
\ Test 1030: Double pass - forward reference
\ REGRESSION: Verifies that Pass 1 (scan-all) builds the word-info
\ table so Pass 2 can resolve forward references correctly.
\ add-one is called before it is defined.
: main 41 add-one 42 = 0= if begin again then ;
: add-one ( n -- n+1 ) 1+ ;

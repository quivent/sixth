\ expect:
\ Test 1033: Double pass - nargs from stack comment
\ REGRESSION: Pass 1 parses ( a b -- c ) to determine nargs=2.
\ Without double pass, forward-ref defaults to nargs=1 and the
\ second argument would be lost.
: main 3 4 add-two 7 = 0= if begin again then ;
: add-two ( a b -- c ) + ;

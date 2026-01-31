\ Test 1017: Literal-op fusion - subtract immediate
\ REGRESSION: Verifies gen-sub-imm correctness for runtime - literal.
: double ( n -- n*2 ) dup + ;
: main 10 double 3 - 17 = 0= if begin again then ;

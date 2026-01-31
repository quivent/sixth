\ Test 1016: Literal-op fusion - add immediate
\ REGRESSION: When one operand is runtime and one is a literal,
\ the compiler should emit a fused add-imm instruction instead of
\ push-tos + gen-lit + gen-add. Verifies correctness of gen-add-imm.
: double ( n -- n*2 ) dup + ;
: main 10 double 7 + 27 = 0= if begin again then ;

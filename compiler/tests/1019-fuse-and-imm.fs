\ Test 1019: Literal-op fusion - AND immediate
\ REGRESSION: Verifies gen-and-imm correctness. Key for arith benchmark.
: inc ( n -- n+1 ) 1+ ;
: main $F0F0 inc $FF00 and $F000 = 0= if begin again then ;

\ expect:
\ Test 1018: Literal-op fusion - multiply immediate
\ REGRESSION: Verifies gen-mul-imm (imul rax, rax, imm32) correctness.
: inc ( n -- n+1 ) 1+ ;
: main 6 inc 6 * 42 = 0= if begin again then ;

\ expect:
\ Test 1020: Literal-op fusion - OR immediate
\ REGRESSION: Verifies gen-or-imm correctness.
: zero ( -- 0 ) 0 ;
: main zero $FF or $FF = 0= if begin again then ;

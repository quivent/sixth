\ Test 1021: Literal-op fusion - XOR immediate
\ REGRESSION: Verifies gen-xor-imm correctness.
: val ( -- n ) $FF ;
: main val $F0 xor $0F = 0= if begin again then ;

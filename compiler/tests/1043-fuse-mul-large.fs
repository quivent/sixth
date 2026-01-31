\ Test 1043: Fusion with large immediate (imul rax, rax, imm32)
\ REGRESSION: gen-mul-imm uses 4-byte immediate. This tests a value
\ larger than 8 bits to ensure the imm32 encoding is correct.
: val ( -- n ) 7 ;
: main val 1000 * 7000 = 0= if begin again then ;
